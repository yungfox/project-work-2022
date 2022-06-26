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
            // creo il client mqtt
            var factory = new MqttFactory();
            client = factory.CreateMqttClient();

            this.options = new MqttClientOptionsBuilder()
                                .WithTcpServer(url)
                                .WithCleanSession()
                                .Build();

            this.topic = topic;

            // istanzio le librerie dei metodi per il salvataggio su mongodb e sulla coda
            this.db = new DBMongo();
            this.queue = new QueueService("localhost", "Parking");
        }

        // metodo per inviare un messaggio tramite mqtt ai sensori
        public async void Send(string payload, string topic)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic($"parking/{topic}")
                                .WithPayload(payload)
                                .WithAtLeastOnceQoS()
                                .Build();

            await client.PublishAsync(message, System.Threading.CancellationToken.None);
        }

        // metodo per eseguire la subscription, elaborare il contenuto dei messaggi e salvarli su mongodb e sulla coda
        public async void Subscribe()
        {
            try
            {
                // eseguo la subscription al topic passato all'istanza della classe
                client.UseConnectedHandler(async e =>
                {
                    Console.WriteLine("connected to the mqtt broker!");
                    await client.SubscribeAsync(new MqttTopicFilterBuilder()
                                                        .WithTopic(topic)
                                                        .Build()
                                                );
                });

                // tento di riconnettermi al broker se in caso di disconnessione
                client.UseDisconnectedHandler(async e =>
                {
                    Console.WriteLine("disconnected from the mqtt broker! trying to reconnect...");
                    while(!client.IsConnected)
                    {
                        Thread.Sleep(5000);
                        await client.ConnectAsync(options, CancellationToken.None);
                    }
                });

                // logica di parsing e smistamento messaggi
                client.UseApplicationMessageReceivedHandler(async e =>
                {
                    // ottengo il messaggio
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    Console.WriteLine($"new message from {e.ClientId} on topic {e.ApplicationMessage.Topic}: {payload}");
                    
                    // creo un timestamp
                    var now = DateTime.UtcNow;

                    // logica di ricezione di un messaggio dai sensori delle piazzole.
                    // se nel payload è presente il campo Status so che il messaggio
                    // proviene da uno dei sensori delle piazzole di parcheggio.
                    if (payload.Contains("Status"))
                    {
                        try
                        {
                            // deserializzo il json del messaggio
                            dynamic json = JsonConvert.DeserializeObject(payload);

                            // creo un nuovo oggetto ParkingSpotMessage partendo
                            // dai dati in ingresso aggiungendo il timestamp
                            ParkingSpotMessage message = new ParkingSpotMessage()
                            {
                                _id = json._id.ToString(),
                                taken = Convert.ToBoolean(json.Status.ToString()),
                                timestamp = now
                            };

                            // aggiorno il database locale di mongodb
                            await db.UpdateParkingSpot(message._id, message.taken, message.timestamp);

                            // serializzo il nuovo oggetto creato in json e lo scrivo sulla coda
                            string statusQueuePayload = JsonConvert.SerializeObject(message);
                            await queue.Send(statusQueuePayload, "ParkingSpots");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    // se il messaggio non proviene dai sensori delle piazzole
                    else
                    {
                        // deserializzo il json del messaggio
                        GenericMessage message = JsonConvert.DeserializeObject<GenericMessage>(payload);

                        switch (message.Dispositivo)
                        {
                            case "ESP32Uscita":
                                // attendo la conferma di validazione da mongodb
                                // e salvo il record in caso positivo
                                bool exitResult = await db.Exit(message._id, now);

                                // compongo il messaggio di risposta per la sbarra di uscita e lo invio
                                string exitResponseJson = $"{{\"stato\":{Convert.ToInt32(exitResult)}}}";
                                Send(exitResponseJson, "exit/gatewaytodevice");

                                if (exitResult)
                                {
                                    try
                                    {
                                        // istanzio un nuovo oggetto ExitMessage usando l'id
                                        // del messaggio e il timestamp attuale
                                        ExitMessage exit = new ExitMessage()
                                        {
                                            _id = message._id,
                                            exitTime = now
                                        };

                                        // serializzo il messaggio e lo scrivo sulla coda
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
                                // attendo la conferma di validazione da mongodb
                                dynamic entryResult = await db.Entry(message._id, now);

                                // compongo il messaggio di risposta per la sbarra di entrata e lo invio
                                string entryResponseJson = $"{{\"stato\":{Convert.ToInt32(entryResult.Status)},\"spot\":{Convert.ToInt32(entryResult.Spot)}}}";
                                Send(entryResponseJson, "entry/gatewaytodevice");

                                if (entryResult.Status)
                                {
                                    try
                                    {
                                        // istanzio un nuovo oggetto EntryMessage usando
                                        // l'id del messaggio e il timestamp attuale
                                        EntryMessage entry = new EntryMessage()
                                        {
                                            _id = message._id,
                                            entryTime = now
                                        };

                                        // serializzo il messaggio e lo scrivo sulla coda
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
                                // attendo la conferma di validazione da mongodb
                                dynamic paymentResult = await db.Payment(message._id, now);

                                // compongo il messaggio di risposta per il totem di pagamento e lo invio
                                string paymentResponseJson = $"{{\"stato\":{Convert.ToInt32(paymentResult.Status)}}}";
                                Send(paymentResponseJson, "payment/gatewaytodevice");

                                if (paymentResult.Status)
                                {
                                    try
                                    { 
                                        // istanzio un nuovo oggetto PaymentMessage usando l'id
                                        // del messaggio, il timestamp attuale e la tariffa
                                        PaymentMessage payment = new PaymentMessage()
                                        {
                                            _id = message._id,
                                            paymentTime = now,
                                            bill = (float)paymentResult.TotalBill
                                        };

                                        // serializzo il messaggio e lo scrivo sulla coda
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

                // apro la connessione con il broker
                await client.ConnectAsync(options);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // algoritmo per la validazione dell'id della piazzola
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