using System;
using System.Text;
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
                                .WithExactlyOnceQoS()
                                .Build();

            await client.PublishAsync(message, System.Threading.CancellationToken.None);
        }

        public void Subscribe()
        {
            try
            {
                client.UseConnectedHandler(e =>
                {
                    Console.WriteLine("connected to the mqtt broker!");
                    client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build()).Wait();
                });

                client.UseDisconnectedHandler(e =>
                {
                    Console.WriteLine("disconnected from the mqtt broker!");
                });

                client.UseApplicationMessageReceivedHandler(async e =>
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    Console.WriteLine($"new message from {e.ClientId} on topic {e.ApplicationMessage.Topic}: {payload}");
                    GenericMessage message = JsonConvert.DeserializeObject<GenericMessage>(payload);

                    var now = DateTime.UtcNow;

                    switch (message.Dispositivo)
                    {
                        case "ESP32Uscita":
                            bool exitResult = await db.Exit(message._id, now);
                            if (exitResult)
                            {
                                ExitMessage exit = new ExitMessage()
                                {
                                    _id = message._id,
                                    exitTime = now
                                };
                                string exitJson = JsonConvert.SerializeObject(exit);

                                await queue.Send(exitJson, "Exit");
                            }
                            break;
                        case "ESP32Entrata":
                            bool entryResult = await db.Entry(message._id, now);
                            if (entryResult)
                            {
                                EntryMessage entry = new EntryMessage() { 
                                    _id = message._id, 
                                    entryTime = now 
                                };
                                string entryJson = JsonConvert.SerializeObject(entry);

                                await queue.Send(entryJson, "Entry");
                            }
                            break;
                        case "RaspberryPagamenti":
                            dynamic paymentResult = await db.Payment(message._id, now);
                            if (paymentResult.Status)
                            {
                                PaymentMessage payment = new PaymentMessage()
                                {
                                    _id = message._id,
                                    paymentTime = now,
                                    bill = paymentResult.TotalBill
                                };
                                string paymentJson = JsonConvert.SerializeObject(payment);

                                await queue.Send(paymentJson, "Payment");
                            }
                            break;
                        //TODO: regex per i sensori delle piazzole
                        case "piazzola":

                            break;
                        default:
                            break;
                    }

                });

                client.ConnectAsync(options).Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private class GenericMessage
        {
            public string _id { get; set; }
            public string Dispositivo { get; set; }
        }
    }
}