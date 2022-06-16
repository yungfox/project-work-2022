using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MqttService(string url, string topic)
        {
            var factory = new MqttFactory();
            client = factory.CreateMqttClient();

            this.options = new MqttClientOptionsBuilder()
                                .WithTcpServer(url)
                                .WithCleanSession()
                                .Build();

            this.topic = topic;
        }

        public void Subscribe()
        {
            try
            {
                client.UseConnectedHandler(e =>
                {
                    Console.WriteLine("connesso");
                    client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build()).Wait();
                });

                client.UseDisconnectedHandler(e =>
                {
                    Console.WriteLine("disconnesso");
                });

                client.UseApplicationMessageReceivedHandler(e =>
                {
                    //var mqttclient = e.ClientId;
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var message_topic = e.ApplicationMessage.Topic;

                    Console.WriteLine($"new message from {e.ClientId} on topic {message_topic}: {payload}");
                });

                client.ConnectAsync(options).Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
