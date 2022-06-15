using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ApplicativoSalvataggioMongoeCoda
{
    class Program
    {
        static void Main(string[] args)
        {
            Connetti();
        }

        static void Connetti()
        {
            var dbParcheggio = new MongoClient("mongodb://127.0.0.1:27017");
            var dbList = dbParcheggio.ListDatabases().ToList();

            Console.WriteLine("The list of databases are:");

            foreach (var item in dbList)
            {
                Console.WriteLine(item);
            }
        }
        
    }
}
