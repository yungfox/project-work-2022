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
            //creo i database in caso non ci siano ancora
            dBMongo.SeedDatabase();

            MqttService mqtt = new MqttService("localhost", "#");
            mqtt.Subscribe();

            //var res = await azureFunctionService.GetUpdatedBillings();
            //await dBMongo.UpdateBilling(res);

            //QueueService queue = new QueueService("localhost", "Parking");
            //queue.Subscribe("Parking");

            Console.ReadLine();
        }
    }
}
