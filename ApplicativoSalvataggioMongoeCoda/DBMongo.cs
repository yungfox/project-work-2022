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
        public float Pagamento(string IDBiglietto, DateTime orariopagamento)
        {
            var collection  = dbParcheggio.GetCollection<Biglietto>("Biglietti");
            //calcolo quanto tempo è passato da quando sono entrato a quando voglio pagare
            var filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDBiglietto);
            var document = collection.Find(filter).First();
            System.TimeSpan diff;
            if (document.OrarioPagamento == Convert.ToDateTime("0001-01-01T00:00:00.000+00:00"))
            {
                //se l'orario pagamento non è mai stato istanziato allora calcolo
                diff = orariopagamento.Subtract(document.OrarioEntrata);
            }
            else
            {
                //se invece è già istanziato vuol dire che l'utente ha già pagato una volta ma ci ha messo troppo tempo ad arrivare all'uscita
                diff = orariopagamento.Subtract(document.OrarioPagamento);
            }
            //prendo i prezzi odierni
            var collection2 = dbParcheggio.GetCollection<Prezzi>("Prezzi");
            var filter2 = Builders<Prezzi>.Filter.Eq("id", 1);
            var document2 = collection2.Find(filter2).First();
            float prezzo = 0;
            switch (diff.TotalMinutes)
            {
                case <= 15:
                    prezzo = 0;
                    break;
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
            update = Builders<Biglietto>.Update.Set("Prezzo", prezzo + document.Prezzo);
            collection.UpdateOne(filter, update);
            return prezzo;
        }
        public bool Uscita(string IDBiglietto, DateTime orariouscita)
        {
            var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
            //calcolo quanto tempo è passato da quando il cliente ha pagato a quando è arrivato alla sbarra
            var filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDBiglietto);
            var document = collection.Find(filter).First();
            System.TimeSpan diff = orariouscita.Subtract(document.OrarioPagamento);
            //se sono passati meno di 15 minuti il cliente può uscire
            if(diff.TotalMinutes <= 15)
            {
                filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDBiglietto);
                var update = Builders<Biglietto>.Update.Set("OrarioUscita", orariouscita);
                collection.UpdateOne(filter, update);
                return true;
            }
            return false;
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
