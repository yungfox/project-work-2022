using ApplicativoSalvataggioMongoeCoda.Models;
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

        public void Entry(string IDTicket)
        {
            try
            {
                var entrytime = DateTime.UtcNow;
                var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
                Biglietto myBiglietto = new()
                {
                    IdBiglietto = IDTicket,
                    OrarioEntrata = entrytime
                };
                collection.InsertOne(myBiglietto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        public float Payment(string IDTicket)
        {
            try
            {
                var paymentime = DateTime.UtcNow;
                var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
                //calcolo quanto tempo è passato da quando sono entrato a quando voglio pagare
                var filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDTicket);
                var document = collection.Find(filter).First();
                System.TimeSpan diff;
                if (DateTime.Compare(document.OrarioPagamento, Convert.ToDateTime("1970-01-01T00:00:00.000+00:00")) < 0)
                {
                    //se l'orario pagamento non è mai stato istanziato allora calcolo
                    diff = paymentime.Subtract(Convert.ToDateTime(document.OrarioEntrata));
                }
                else
                {
                    //se invece è già istanziato vuol dire che l'utente ha già pagato una volta ma ci ha messo troppo tempo ad arrivare all'uscita
                    diff = paymentime.Subtract(Convert.ToDateTime(document.OrarioPagamento));
                }
                //prendo i prezzi odierni
                var collection2 = dbParcheggio.GetCollection<Prezzi>("Prezzi");
                var filter2 = Builders<Prezzi>.Filter.Eq("id", 1);
                var document2 = collection2.Find(filter2).First();
                float price = 0;
                switch (diff.TotalMinutes)
                {
                    case <= 15:
                        price = 0;
                        break;
                    case <= 30:
                        price = document2.mezzora;
                        break;
                    case <= 60:
                        price = document2.unora;
                        break;
                    case <= 180:
                        price = document2.treore;
                        break;
                    case <= 360:
                        price = document2.seiore;
                        break;
                    case <= 1440:
                        price = Convert.ToInt32(diff.TotalDays) * document2.giornaliero;
                        break;
                    default:
                        break;
                }
                filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDTicket);
                var update = Builders<Biglietto>.Update.Set("OrarioPagamento", paymentime);
                collection.UpdateOne(filter, update);
                update = Builders<Biglietto>.Update.Set("Prezzo", price + document.Prezzo);
                collection.UpdateOne(filter, update);
                return price;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
        public bool Uscita(string IDTicket)
        {
            try
            {
                var exitime = DateTime.UtcNow;
                var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
                //calcolo quanto tempo è passato da quando il cliente ha pagato a quando è arrivato alla sbarra
                var filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDTicket);
                var document = collection.Find(filter).First();
                System.TimeSpan diff = exitime.Subtract(document.OrarioPagamento);
                //se sono passati meno di 15 minuti dal pagamento il cliente può uscire
                if (diff.TotalMinutes <= 15)
                {
                    filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDTicket);
                    var update = Builders<Biglietto>.Update.Set("OrarioUscita", exitime);
                    collection.UpdateOne(filter, update);
                    return true;
                }
                //in caso contrario dovrà tornare al totem e pagare
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            
            
        }
        public float GetPrezzo(string IDTicket)
        {
            try
            {
                var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
                var filter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDTicket);
                var document = collection.Find(filter).First();
                return (float)document.Prezzo;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            
        }
        public void AggiornaCosti(string tempistica, float prezzodaaggiornare)
        {
            try
            {
                var collection = dbParcheggio.GetCollection<Prezzi>("Prezzi");
                var filter = Builders<Prezzi>.Filter.Eq("id", 1);
                var update = Builders<Prezzi>.Update.Set(tempistica, prezzodaaggiornare);
                collection.UpdateMany(filter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
