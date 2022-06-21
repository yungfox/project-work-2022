using System;
using ApplicativoSalvataggioMongoeCoda.Services;

namespace ScritturaDbAzure
{
    internal class Program
    {
        const string QUEUE = "Parking";

        static void Main(string[] args)
        {
            QueueService service = new QueueService("localhost", QUEUE);
            service.Subscribe(QUEUE);

            Console.ReadLine();
        }
    }
}
