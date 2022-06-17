﻿using System;
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
            this.coda = coda;
            factory = new ConnectionFactory() { HostName = req_url };
            connection = factory.CreateConnection();

            channel = connection.CreateModel();
            channel.QueueDeclare(
                        queue: "Parking",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

            azureClient = ServiceClient.CreateFromConnectionString(Secrets.AZURE_CONNECTION_STRING);

            Console.WriteLine("connected to the amqp queue!");
        }

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
                switch (args.Exchange)
                {
                    case "Entry":
                        EntryMessage entry = JsonConvert.DeserializeObject<EntryMessage>(payload);

                        Console.WriteLine($"dopo la deserializzazione: {entry.entryTime}");

                        using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                        using (SqlCommand cmd = new SqlCommand()
                        {
                            Connection = con,
                            CommandType = System.Data.CommandType.Text,
                            CommandText = "INSERT INTO tblTicket (IdTicket, EntryTime) VALUES (@id, @time)"
                        })
                        {
                            cmd.Parameters.AddWithValue("id", entry._id);
                            cmd.Parameters.AddWithValue("time", entry.entryTime);

                            con.Open();

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                channel.BasicAck(args.DeliveryTag, false);
                            }
                        }
                        break;
                    case "Payment":
                        PaymentMessage payment = JsonConvert.DeserializeObject<PaymentMessage>(payload);

                        using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                        using (SqlCommand cmd = new SqlCommand()
                        {
                            Connection = con,
                            CommandType = System.Data.CommandType.Text,
                            CommandText = "UPDATE tblTicket SET PaymentTime=@time, Bill=@bill WHERE IdTicket=@time"
                        })
                        {
                            cmd.Parameters.AddWithValue("id", payment._id);
                            cmd.Parameters.AddWithValue("time", payment.paymentTime);
                            cmd.Parameters.AddWithValue("bill", payment.bill);

                            con.Open();

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                channel.BasicAck(args.DeliveryTag, false);
                            }
                        }
                        break;
                    case "Exit":
                        ExitMessage exit = JsonConvert.DeserializeObject<ExitMessage>(payload);

                        using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                        using (SqlCommand cmd = new SqlCommand()
                        {
                            Connection = con,
                            CommandType = System.Data.CommandType.Text,
                            CommandText = $"UPDATE tblTicket SET ExitTime=@time WHERE IdTicket=@id"
                        })
                        {
                            cmd.Parameters.AddWithValue("id", exit._id);
                            cmd.Parameters.AddWithValue("exit", exit.exitTime);

                            con.Open();

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                channel.BasicAck(args.DeliveryTag, false);
                            }
                        }
                        break;
                    case "ParkingSpots":
                        ParkingSpotMessage spot = JsonConvert.DeserializeObject<ParkingSpotMessage>(payload);

                        using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                        using (SqlCommand cmd = new SqlCommand()
                        {
                            Connection = con,
                            CommandType = System.Data.CommandType.Text,
                            CommandText = $"UPDATE tblPiazzole SET Timestamp=@time, Status=@status WHERE Id=@id"
                        })
                        {
                            cmd.Parameters.AddWithValue("id", spot._id);
                            cmd.Parameters.AddWithValue("status", spot.taken);
                            cmd.Parameters.AddWithValue("time", spot.timestamp);

                            con.Open();

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                channel.BasicAck(args.DeliveryTag, false);
                                Console.WriteLine("successfully updated the parking spot on db!");
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
        }

        private async Task WriteOnCloud(string payload)
        {
            Message message = new Message(Encoding.UTF8.GetBytes(payload));
            await azureClient.SendAsync("Parcheggio", message);
        }
    }
}
