using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace ApplicativoSalvataggioMongoeCoda.Services
{
    public class MqttService
    {
        private IMqttClient client;
        private IMqttClientOptions options;
        private string topic;
        private DBMongo db;

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
        }

        public void Subscribe()
        {
            try
            {
                client.UseConnectedHandler(e =>
                {
                    Console.WriteLine("connected to the broker");
                    client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build()).Wait();
                });

                client.UseDisconnectedHandler(e =>
                {
                    Console.WriteLine("disconnected from the broker");
                });

                client.UseApplicationMessageReceivedHandler(async e =>
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    Console.WriteLine($"new message from {e.ClientId} on topic {e.ApplicationMessage.Topic}: {payload}");
                    IncomingMessage message = JsonConvert.DeserializeObject<IncomingMessage>(payload);

                    switch (message.Dispositivo)
                    {
                        case "ESP32Uscita":
                            var now = DateTime.UtcNow;
                            //await db.Exit(message._id);
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

        private class IncomingMessage
        {
            public string _id { get; set; }
            public string Dispositivo { get; set; }
        }
    }
}
