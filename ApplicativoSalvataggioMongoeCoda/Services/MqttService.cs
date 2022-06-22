using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using ApplicativoSalvataggioMongoeCoda.Models.Messages;

namespace ApplicativoSalvataggioMongoeCoda.Services
{
    public class MqttService
    {
        private IMqttClient client;
        private IMqttClientOptions options;
        private string topic;
        private DBMongo db;
        private QueueService queue;

        public MqttService(string url, string topic)
        {
            var factory = new MqttFactory();
            client = factory.CreateMqttClient();

            this.options = new MqttClientOptionsBuilder()
                                .WithTcpServer(url)
                                .WithCleanSession()
                                .Build();

            this.topic = topic;
            this.db = new DBMongo();
            this.queue = new QueueService("localhost", "Parking");
        }

        public async void Send(string payload, string topic)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic($"parking/{topic}")
                                .WithPayload(payload)
                                .WithAtLeastOnceQoS()
                                .Build();

            await client.PublishAsync(message, System.Threading.CancellationToken.None);
        }

        public async void Subscribe()
        {
            try
            {
                client.UseConnectedHandler(async e =>
                {
                    Console.WriteLine("connected to the mqtt broker!");
                    await client.SubscribeAsync(new MqttTopicFilterBuilder()
                                                        .WithTopic(topic)
                                                        .Build()
                                                );
                });

                client.UseDisconnectedHandler(async e =>
                {
                    Console.WriteLine("disconnected from the mqtt broker! trying to reconnect...");
                    while(!client.IsConnected)
                    {
                        Thread.Sleep(5000);
                        await client.ConnectAsync(options, CancellationToken.None);
                    }
                });

                client.UseApplicationMessageReceivedHandler(async e =>
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    Console.WriteLine($"new message from {e.ClientId} on topic {e.ApplicationMessage.Topic}: {payload}");
                    
                    var now = DateTime.UtcNow;

                    if (payload.Contains("Status"))
                    {
                        try
                        {
                            ParkingSpotMessage message = JsonConvert.DeserializeObject<ParkingSpotMessage>(payload);
                            await db.UpdateParkingSpot(message._id, message.taken, message.timestamp);
                            await queue.Send(payload, "ParkingSpots");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        GenericMessage message = JsonConvert.DeserializeObject<GenericMessage>(payload);

                        switch (message.Dispositivo)
                        {
                            case "ESP32Uscita":
                                dynamic exitResult = await db.Exit(message._id, now);
                                string exitResponseJson = $"{{\"stato\":{Convert.ToInt32(exitResult.Status)},\"spot\":{Convert.ToInt32(exitResult.Spot)}}}";

                                Send(exitResponseJson, "exit/gatewaytodevice");

                                if (exitResult.Status)
                                {
                                    try
                                    {
                                        ExitMessage exit = new ExitMessage()
                                        {
                                            _id = message._id,
                                            exitTime = now
                                        };
                                        string exitJson = JsonConvert.SerializeObject(exit);

                                        await queue.Send(exitJson, "Exit");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                break;

                            case "ESP32Entrata":
                                bool entryResult = await db.Entry(message._id, now);
                                string entryResponseJson = $"{{\"stato\":{Convert.ToInt32(entryResult)}}}";

                                Send(entryResponseJson, "entry/gatewaytodevice");

                                if (entryResult)
                                {
                                    try
                                    {
                                        EntryMessage entry = new EntryMessage()
                                        {
                                            _id = message._id,
                                            entryTime = now
                                        };
                                        string entryJson = JsonConvert.SerializeObject(entry);

                                        await queue.Send(entryJson, "Entry");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                break;

                            case "TotemPagamento":
                                dynamic paymentResult = await db.Payment(message._id, now);
                                string paymentResponseJson = $"{{\"stato\":{Convert.ToInt32(paymentResult.Status)}}}";

                                Send(paymentResponseJson, "payment/gatewaytodevice");

                                if (paymentResult.Status)
                                {
                                    try
                                    {
                                        PaymentMessage payment = new PaymentMessage()
                                        {
                                            _id = message._id,
                                            paymentTime = now,
                                            bill = (float)paymentResult.TotalBill
                                        };
                                        string paymentJson = JsonConvert.SerializeObject(payment);

                                        await queue.Send(paymentJson, "Payment");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                break;

                            default:
                                break;
                        }
                    }

                });

                await client.ConnectAsync(options);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private bool CheckId(string id)
        {
            if (id.Length == 3 && int.TryParse(id, out _))
            {
                int id_num = Convert.ToInt32(id);

                if ((id_num >= 1 && id_num <= 50) || (id_num >= 101 && id_num <= 150))
                    return true;
            }
            return false;
        }

        private class GenericMessage
        {
            public string _id { get; set; }
            public string Dispositivo { get; set; }
        }
    }
}