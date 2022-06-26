using System;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using ApplicativoSalvataggioMongoeCoda.Models.Messages;

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
            // creo la connessione alla coda
            this.coda = coda;
            factory = new ConnectionFactory() {
                HostName = req_url,
                UserName = Secrets.RABBITMQ_USERNAME,
                Password = Secrets.RABBITMQ_PASSWORD
            };

            connection = factory.CreateConnection();

            channel = connection.CreateModel();

            // dichiaro la coda
            channel.QueueDeclare(
                        queue: coda,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

            // creo il client di azure iothub per la scrittura di messaggi
            azureClient = ServiceClient.CreateFromConnectionString(Secrets.AZURE_CONNECTION_STRING);

            Console.WriteLine("connected to the amqp queue!");
        }

        // metodo per eseguire il publish sulla coda attraverso l'exchange passato come parametro
        public Task Send(string dati, string exchange)
        {
            return Task.Run(() =>
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
            });
        }

        // metodo per eseguire l'iscrizione alla coda, elaborare il contenuto
        //  dei messaggi e scriverli su sql server e azure iothub
        public void Subscribe(string coda)
        {
            // dichiaro la coda a cui fare il subscribe
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
                    // ottengo il messaggio dalla coda
                    var body = args.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"new message from exchange {args.Exchange}: {message}");

                    // scrivo il contenuto del messaggio su sql server e su iothub
                    await WriteOnDbAsync(message, args);
                    await WriteOnCloud(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };

            // disabilito l'ack automatico del messaggio ed eseguo il consume
            channel.BasicConsume(
                        queue: coda,
                        autoAck: false,
                        consumer: consumer
                    );
        }

        // logica di scrittura dei messaggi su sql server in base all'exchange di provenienza
        private Task WriteOnDbAsync(string payload, BasicDeliverEventArgs args)
        {
            return Task.Run(() =>
            {
                // controllo da quale exchange proviene il messaggio
                switch (args.Exchange)
                {
                    case "Entry":
                        try
                        {
                            // deserializzo il messaggio
                            EntryMessage entry = JsonConvert.DeserializeObject<EntryMessage>(payload);

                            // aggiungo un nuovo record alla tabella tblTicket con id e data di ingresso
                            using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                            using (SqlCommand cmd = new SqlCommand()
                            {
                                Connection = con,
                                CommandType = System.Data.CommandType.Text,
                                CommandText = "INSERT INTO tblTicket (IdTicket, EntryTime) VALUES (@id, @time)"
                            })
                            {
                                // aggiungo i parametri ottenuti dal messaggio
                                cmd.Parameters.AddWithValue("id", entry._id);
                                cmd.Parameters.AddWithValue("time", entry.entryTime);

                                con.Open();

                                // ottengo il risultato dalla query
                                int result = cmd.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    // eseguo l'ack del messaggio solo se la query va a buon fine
                                    channel.BasicAck(args.DeliveryTag, false);
                                    Console.WriteLine("successfully inserted the entry on db!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CheckForPrimaryKeyViolation(ex, args);
                            Console.WriteLine(ex.Message);
                        }

                        break;

                    case "Payment":
                        try
                        {
                            // deserializzo il messaggio
                            PaymentMessage payment = JsonConvert.DeserializeObject<PaymentMessage>(payload);

                            // aggiungo la data di pagamento al record corrispondente all'id del messaggio
                            using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                            using (SqlCommand cmd = new SqlCommand()
                            {
                                Connection = con,
                                CommandType = System.Data.CommandType.Text,
                                CommandText = "UPDATE tblTicket SET PaymentTime=@time, Bill=@bill WHERE IdTicket=@id"
                            })
                            {
                                // aggiungo i parametri ottenuti dal messaggio
                                cmd.Parameters.AddWithValue("id", payment._id);
                                cmd.Parameters.AddWithValue("time", payment.paymentTime);
                                cmd.Parameters.AddWithValue("bill", payment.bill);

                                con.Open();

                                // ottengo il risultato dalla query
                                int result = cmd.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    // eseguo l'ack del messaggio solo se la query va a buon fine
                                    channel.BasicAck(args.DeliveryTag, false);
                                    Console.WriteLine("successfully updated the payment state on db!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CheckForPrimaryKeyViolation(ex, args);
                            Console.WriteLine(ex.Message);
                        }

                        break;

                    case "Exit":
                        try
                        {
                            // deserializzo il messaggio
                            ExitMessage exit = JsonConvert.DeserializeObject<ExitMessage>(payload);

                            // aggiungo l'ora di uscita al record corrispondente all'id del messaggio
                            using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                            using (SqlCommand cmd = new SqlCommand()
                            {
                                Connection = con,
                                CommandType = System.Data.CommandType.Text,
                                CommandText = $"UPDATE tblTicket SET ExitTime=@time WHERE IdTicket=@id"
                            })
                            {
                                // aggiungo i parametri ottenuti dal messaggio
                                cmd.Parameters.AddWithValue("id", exit._id);
                                cmd.Parameters.AddWithValue("time", exit.exitTime);

                                con.Open();

                                // ottengo il risultato dalla query
                                int result = cmd.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    // eseguo l'ack del messaggio solo se la query va a buon fine
                                    channel.BasicAck(args.DeliveryTag, false);
                                    Console.WriteLine("successfully set the exit time on db!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CheckForPrimaryKeyViolation(ex, args);
                            Console.WriteLine(ex.Message);
                        }

                        break;

                    case "ParkingSpots":
                        try
                        {
                            // deserializzo il messaggio
                            ParkingSpotMessage spot = JsonConvert.DeserializeObject<ParkingSpotMessage>(payload);

                            // aggiorno lo stato e l'ora della piazzola corrispondente all'id del messaggio
                            using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                            using (SqlCommand cmd = new SqlCommand()
                            {
                                Connection = con,
                                CommandType = System.Data.CommandType.Text,
                                CommandText = $"UPDATE tblParkingSpot SET Timestamp=@time, Status=@status WHERE Id=@id"
                            })
                            {
                                // aggiungo i parametri ottenuti dal messaggio
                                cmd.Parameters.AddWithValue("id", spot._id);
                                cmd.Parameters.AddWithValue("status", spot.taken);
                                cmd.Parameters.AddWithValue("time", spot.timestamp);

                                con.Open();

                                // ottengo il risultato dalla query
                                int result = cmd.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    // eseguo l'ack del messaggio solo se la query va a buon fine
                                    channel.BasicAck(args.DeliveryTag, false);
                                    Console.WriteLine("successfully updated the parking spot state on db!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CheckForPrimaryKeyViolation(ex, args);
                            Console.WriteLine(ex.Message);
                        }

                        break;

                    default:
                        break;
                }
            });
        }

        // metodo per scrivere il messaggio su azure iothub
        private async Task WriteOnCloud(string payload)
        {
            try
            {
                Message message = new Message(Encoding.UTF8.GetBytes(payload));
                await azureClient.SendAsync("ParkingIoT", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // metodo per controllare se l'eccezione lanciata da sql server è una violazione di chiave primaria.
        // in tal caso eseguo l'ack del messaggio in quanto resterebbe sulla coda generando l'eccezione ogni volta
        private void CheckForPrimaryKeyViolation(Exception ex, BasicDeliverEventArgs args)
        {
            if (ex is SqlException dbEx)
            {
                if (dbEx.Number == 2627)
                    channel.BasicAck(args.DeliveryTag, false);
            }
        }
    }
}
