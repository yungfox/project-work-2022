using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace ApplicativoSalvataggioMongoeCoda
{
    class Program
    {
        DBMongo dBMongo = new DBMongo();
        static void Main(string[] args)
        {
            //InserisciEntrata();
            DBMongo mydBMongo = new();

            //mydBMongo.Entrata("1111", Convert.ToDateTime("2022-06-15T11:19:35.397+00:00"));
            mydBMongo.Pagamento("1111", Convert.ToDateTime("2022-06-15T11:21:35.397+00:00"));
            //mydBMongo.Uscita("1111", Convert.ToDateTime("2022-06-15T11:19:35.397+00:00"));

            //mydBMongo.AggiornaCosti("mezzora",125);
        }        
    }
}
