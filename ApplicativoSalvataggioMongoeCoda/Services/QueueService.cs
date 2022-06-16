using System;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;

namespace ApplicativoSalvataggioMongoeCoda.Services
{
    public class QueueService
    {
        private ConnectionFactory factory;
        private readonly string coda;
        private IConnection connection;
        private IModel channel;
        private ServiceClient azureClient;

        public QueueService(string req_url, string coda)
        {
            this.coda = coda;
            factory = new ConnectionFactory() { HostName = req_url };
            connection = factory.CreateConnection();

            channel = connection.CreateModel();
            channel.QueueDeclare(
                        queue: "Parcheggio",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

            azureClient = ServiceClient.CreateFromConnectionString(Secrets.AZURE_CONNECTION_STRING);

            Console.WriteLine("connesso!");
        }

        public void Send(string dati, string exchange)
        {
            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic);

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

        public void Subscribe(string coda)
        {
            channel.QueueDeclare(
                        queue: coda,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"new message from exchange {args.Exchange}: {message}");

                    await WriteOnDbAsync(message, args);

                    await WriteOnCloud(message);
                    //channel.BasicAck(args.DeliveryTag, false);      // DEBUG - COMMENTA SE WRITEONDBASYNC È DECOMMENTATO!!!!!!
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };

            channel.BasicConsume(
                        queue: coda,
                        autoAck: false,
                        consumer: consumer
                    );
        }

        private Task WriteOnDbAsync(string payload, BasicDeliverEventArgs args)
        {
            return Task.Run(() =>
            {
                if(args.Exchange == "Biglietti")
                {
                    MongoTicket biglietto = JsonConvert.DeserializeObject<MongoTicket>(payload);

                    using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                    using (SqlCommand cmd = new SqlCommand()
                    {
                        Connection = con,
                        CommandType = System.Data.CommandType.Text,
                        CommandText = $"INSERT INTO tblBiglietti (IdBiglietto, OrarioEntrata, OrarioPagamento, OrarioUscita, Prezzo) VALUES (@biglietto, @entrata, @pagamento, @uscita, @prezzo)"
                    })
                    {
                        cmd.Parameters.AddWithValue("biglietto", biglietto._id);
                        cmd.Parameters.AddWithValue("entrata", biglietto.OrarioEntrata.Date);
                        cmd.Parameters.AddWithValue("pagamento", biglietto.OrarioPagamento.Date);
                        cmd.Parameters.AddWithValue("uscita", biglietto.OrarioUscita.Date);
                        cmd.Parameters.AddWithValue("prezzo", biglietto.Prezzo);

                        con.Open();

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                            Console.WriteLine("successfully inserted the ticket on db!");
                        }
                    }
                }
                else if(args.Exchange == "Piazzole")
                {
                    MongoParkingSpot spot = JsonConvert.DeserializeObject<MongoParkingSpot>(payload);

                    using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                    using (SqlCommand cmd = new SqlCommand()
                    {
                        Connection = con,
                        CommandType = System.Data.CommandType.Text,
                        CommandText = $"UPDATE tblPiazzole SET Orario='{spot.Orario}', Stato='{spot.Stato}' WHERE Id={spot._id}"
                    })
                    {
                        con.Open();

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                            Console.WriteLine("successfully updated the parking spot on db!");
                        }
                    }
                }
            });
        }

        #region UTILITY CLASSES
        private async Task WriteOnCloud(string payload)
        {
            Message message = new Message(Encoding.UTF8.GetBytes(payload));
            await azureClient.SendAsync("Parcheggio", message);
        }

        private class OrarioEntrata
        {
            [JsonProperty("$date")]
            public DateTime Date { get; set; }
        }

        private class OrarioPagamento
        {
            [JsonProperty("$date")]
            public DateTime Date { get; set; }
        }

        private class OrarioUscita
        {
            [JsonProperty("$date")]
            public DateTime Date { get; set; }
        }

        private class MongoTicket
        {
            public string _id { get; set; }
            public OrarioEntrata OrarioEntrata { get; set; }
            public OrarioPagamento OrarioPagamento { get; set; }
            public OrarioUscita OrarioUscita { get; set; }
            public int Prezzo { get; set; }
        }

        private class Date
        {
            [JsonProperty("$numberLong")]
            public string NumberLong { get; set; }
        }

        private class Orario
        {
            [JsonProperty("$date")]
            public Date Date { get; set; }
        }

        private class MongoParkingSpot
        {
            public string _id { get; set; }
            public Orario Orario { get; set; }
            public bool Stato { get; set; }
        }
        #endregion
    }
}
