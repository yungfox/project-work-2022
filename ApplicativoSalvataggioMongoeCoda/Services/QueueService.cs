using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ApplicativoSalvataggioMongoeCoda.Services
{
    public class QueueService
    {
        private ConnectionFactory factory;
        private readonly string exchange;
        private IConnection connection;

        public QueueService(string req_url, string exchange)
        {
            this.exchange = exchange;
            factory = new ConnectionFactory() { HostName = req_url };
            connection = factory.CreateConnection();
        }

        public void Send(string dati, string coda)
        {
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic);

                channel.QueueDeclare(
                            queue: coda,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null
                        );

                channel.QueueBind(coda, exchange, "def", null);

                var message = dati;
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(
                            exchange: exchange,
                            routingKey: "def",
                            basicProperties: null,
                            body: body
                        );
            }
        }

        public void Subscribe(string coda)
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                            queue: coda,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null
                        );

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, args) =>
                {
                    var body = args.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"new message from queue {coda}: {message}");
                };

                channel.BasicConsume(
                            queue: coda,
                            autoAck: true,
                            consumer: consumer
                        );
            }
        }
    }
}
