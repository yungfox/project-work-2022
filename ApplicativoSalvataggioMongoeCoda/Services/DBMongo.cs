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

        public Task Entry(string IDTicket)
        {
            return Task.Run(() =>
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
            });  
        }
        public Task<float> Payment(string IDTicket)
        {
            return Task.Run(() =>
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
                    var update = Builders<Biglietto>.Update.Set("OrarioPagamento", paymentime).Set("Prezzo", price + document.Prezzo);
                    collection.UpdateOne(filter, update);
                    return price;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return -1;
                }
            });
        }
        public Task<bool> Exit(string IDTicket)
        {
            return Task.Run(() =>
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
                        //non è necessario scrivere l'uscita visto che viene cancellata subito
                        DeleteRecordTicket(IDTicket);
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
            });        
        }
        public Task<float> GetPrice(string IDTicket)
        {
            return Task.Run(() =>
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
            });
        }
        public Task UpdateBilling(string timing, float billtoupdate)
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParcheggio.GetCollection<Prezzi>("Prezzi");
                    var filter = Builders<Prezzi>.Filter.Eq("id", 1);
                    var update = Builders<Prezzi>.Update.Set(timing, billtoupdate);
                    collection.UpdateOne(filter, update);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
        public Task<Boolean> DeleteRecordTicket(string IDTicket)
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParcheggio.GetCollection<Biglietto>("Biglietti");
                    var deleteFilter = Builders<Biglietto>.Filter.Eq("IdBiglietto", IDTicket);
                    collection.DeleteOne(deleteFilter);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            });
        }

        public Task UpdateParkingSpot(string ID, Boolean Status)
        {
            return Task.Run(() =>
            {
                try
                {
                    var time = DateTime.UtcNow;
                    var collection = dbParcheggio.GetCollection<Piazzola>("Piazzole");
                    var filter = Builders<Piazzola>.Filter.Eq("_id", ID);
                    var update = Builders<Piazzola>.Update.Set("Stato", Status).Set("Orario",time);
                    collection.UpdateOne(filter, update);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
        public Task CreateParkingSpot()
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParcheggio.GetCollection<Piazzola>("Piazzole");
                    if (collection.Count(_ => true) == 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 50; j++)
                            {
                                Piazzola myPiazzola;
                                if (j<10)
                                {
                                    myPiazzola = new()
                                    {
                                        Id = i + "0" + j,
                                        IdPiano = i,
                                        IdPiazzola = j
                                    };
                                }
                                else
                                {
                                    myPiazzola = new()
                                    {
                                        Id = i + "" + j,
                                        IdPiano = i,
                                        IdPiazzola = j
                                    };
                                }
                                collection.InsertOne(myPiazzola);
                            }
                        }
                        Console.WriteLine("finito creazione");
                    }
                    else
                    {
                        Console.WriteLine("database già popolato"); 
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
        public Task DropTable()
        {
            return Task.Run(() =>
            {
                var collection = dbParcheggio.GetCollection<Piazzola>("Piazzole");
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 50; j++)
                    {
                        if (j < 10)
                        {
                            var deleteFilter = Builders<Piazzola>.Filter.Eq("_id", i + "0" + j);
                            collection.DeleteOne(deleteFilter);
                        }
                        else
                        {
                            var deleteFilter = Builders<Piazzola>.Filter.Eq("_id", i + "" + j);
                            collection.DeleteOne(deleteFilter);
                        }
                    }
                }
                Console.WriteLine("finito eliminazione");
            });
        }
    }
}
