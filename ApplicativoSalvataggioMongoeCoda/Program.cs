using System;
using System.Threading.Tasks;
using ApplicativoSalvataggioMongoeCoda.Services;

namespace ApplicativoSalvataggioMongoeCoda
{
    class Program
    {
        static DBMongo dBMongo = new DBMongo();
        static AzureFunctionService azureFunctionService = new AzureFunctionService();
        static async Task Main(string[] args)
        {
            MqttService mqtt = new MqttService("172.16.5.4", "#");
            mqtt.Subscribe();

            //var res = await azureFunctionService.GetUpdatedBillings();
            //await dBMongo.UpdateBilling(res);

            //string test = $"{{\"_id\":\"55555555\",\"OrarioEntrata\":{{\"$date\":\"2022-06-15T11:19:35.397Z\"}},\"OrarioPagamento\":{{\"$date\":\"2022-06-15T11:21:35.398Z\"}},\"OrarioUscita\":{{\"$date\":\"1970-01-01T00:00:00.000Z\"}},\"Prezzo\":90}}";

            QueueService queue = new QueueService("localhost", "Parking");
            //queue.Send(test, "Biglietti");
            queue.Subscribe("Parking");

            Console.ReadLine();
        }
    }
}
