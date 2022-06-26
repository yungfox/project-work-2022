using System;
using ApplicativoSalvataggioMongoeCoda.Services;

namespace ScritturaDbAzure
{
    internal class Program
    {
        const string QUEUE = "Parking";

        static void Main(string[] args)
        {
            // eseguo la subscription alla coda Parking
            QueueService service = new QueueService("localhost", QUEUE);
            service.Subscribe(QUEUE);

            Console.ReadLine();
        }
    }
}
