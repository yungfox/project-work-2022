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

            //QueueService queue = new QueueService("localhost", "Parking");
            //queue.Subscribe("Parking");

            Console.ReadLine();
        }
    }
}
