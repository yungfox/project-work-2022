using System;
using ApplicativoSalvataggioMongoeCoda.Services;

namespace ApplicativoSalvataggioMongoeCoda
{
    class Program
    {
        DBMongo dBMongo = new DBMongo();
        static void Main(string[] args)
        {
            MqttService mqtt = new MqttService("localhost", "#");
            mqtt.Subscribe();

            //string test = $"{{\"_id\":\"55555555\",\"OrarioEntrata\":{{\"$date\":\"2022-06-15T11:19:35.397Z\"}},\"OrarioPagamento\":{{\"$date\":\"2022-06-15T11:21:35.398Z\"}},\"OrarioUscita\":{{\"$date\":\"1970-01-01T00:00:00.000Z\"}},\"Prezzo\":90}}";

            //QueueService queue = new QueueService("localhost", "Parcheggio");
            //queue.Send(test, "Biglietti");

            Console.ReadLine();
        }
    }
}
