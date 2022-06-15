using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda
{
    class DBMongo
    {
        public MongoClient client;
        public IMongoDatabase dbParcheggio;
        //public IMongoCollection <Biglietto> collection;

        public DBMongo()
        {
            this.client = new MongoClient("mongodb://127.0.0.1:27017");
            this.dbParcheggio = client.GetDatabase("Parcheggio");
        }

        public void Entrata(string IDBiglietto, DateTime orarioentrata)
        {
            var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
            Biglietto myBiglietto = new Biglietto
            {
                IdBiglietto = IDBiglietto,
                OrarioEntrata = orarioentrata
            };
            collection.InsertOne(myBiglietto);
        }
        public void PrendiValore()
        {
            var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
            var filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", "55555555");
            var document = collection.Find(filter).First();
            Console.WriteLine(document.IdBiglietto);
            Console.WriteLine(document.OrarioEntrata);
        }
        public void Pagamento(string IDBiglietto, DateTime orariopagamento)
        {
            var collection  = dbParcheggio.GetCollection<Biglietto>("Biglietti");
            //calcolo quanto tempo è passato da quando sono entrato a quando voglio pagare
            var filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDBiglietto);
            var document = collection.Find(filter).First();
            System.TimeSpan diff = orariopagamento.Subtract(document.OrarioEntrata);
            //prendo i prezzi odierni
            Console.WriteLine("mi connetto al secondo db");
            var collection2 = dbParcheggio.GetCollection<Prezzi>("Prezzi");
            var filter2 = Builders<Prezzi>.Filter.Eq("id", 1);
            var document2 = collection2.Find(filter2).First();
            Console.WriteLine("mezzora: "+document2.mezzora);
            float prezzo = 0;
            switch (diff.TotalMinutes)
            {
                case <= 30:
                    prezzo = document2.mezzora;
                    break;
                case <= 60:
                    prezzo = document2.unora;
                    break;
                case <= 180:
                    prezzo = document2.treore;
                    break;
                case <= 360:
                    prezzo = document2.seiore;
                    break;
                case <= 1440:
                    prezzo = Convert.ToInt32(diff.TotalDays) * document2.giornaliero;
                    break;
                default:
                    break;
            }
            filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDBiglietto);
            var update = Builders<Biglietto>.Update.Set("OrarioPagamento", orariopagamento);
            collection.UpdateOne(filter, update);
            update = Builders<Biglietto>.Update.Set("Prezzo", prezzo);
            collection.UpdateOne(filter, update);
        }
        public void Uscita(string IDBiglietto, DateTime orariouscita)
        {
            var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
            var filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDBiglietto);
            var update = Builders<Biglietto>.Update.Set("OrarioUscita", orariouscita);
            collection.UpdateOne(filter, update);
        }
        public void AggiornaCosti(string tempistica, float prezzodaaggiornare)
        {
            var collection = dbParcheggio.GetCollection<Prezzi>("Prezzi");
            var filter = Builders<Prezzi>.Filter.Eq("id", 1);
            var update = Builders<Prezzi>.Update.Set(tempistica, prezzodaaggiornare);
            collection.UpdateMany(filter, update);
        }
        public void CreaCosti()
        {
            var collection = dbParcheggio.GetCollection<Prezzi>("Prezzi");
            Prezzi myBiglietto = new Prezzi
            {
                id = 1,
                mezzora = 0,
                unora = 0,
                treore = 0,
                seiore = 0,
                giornaliero = 0
            };
            collection.InsertOne(myBiglietto);
        }
    }
}
