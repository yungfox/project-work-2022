﻿using ApplicativoSalvataggioMongoeCoda.Models;
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
        public IMongoDatabase dbParking;
        //public IMongoCollection <Biglietto> collection;

        public DBMongo()
        {
            this.client = new MongoClient("mongodb://127.0.0.1:27017");
            this.dbParking = client.GetDatabase("Parking");
        }

        public Task<bool> Entry(string IDTicket, DateTime entrytime)
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParking.GetCollection<Ticket>("Ticket");
                    Ticket myTicket = new()
                    {
                        IdTicket = IDTicket,
                        EntryTime = entrytime
                    };
                    collection.InsertOne(myTicket);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            });
        }
        public Task<dynamic> Payment(string IDTicket, DateTime paymentime)
        {
            return Task.Run(() =>
            {
                dynamic res;
                try
                {
                    var collection = dbParking.GetCollection<Ticket>("Ticket");
                    //calcolo quanto tempo è passato da quando sono entrato a quando voglio pagare
                    var filter = Builders<Ticket>.Filter.Eq("_id", IDTicket);
                    var document = collection.Find(filter).First();
                    System.TimeSpan diff;
                    if (DateTime.Compare(document.PaymentTime, Convert.ToDateTime("1970-01-01T00:00:00.000+00:00")) < 0)
                    {
                        //se l'orario pagamento non è mai stato istanziato allora calcolo
                        diff = paymentime.Subtract(Convert.ToDateTime(document.EntryTime));
                    }
                    else
                    {
                        //se invece è già istanziato vuol dire che l'utente ha già pagato una volta ma ci ha messo troppo tempo ad arrivare all'uscita
                        diff = paymentime.Subtract(Convert.ToDateTime(document.PaymentTime));
                    }
                    //prendo i prezzi odierni
                    var collection2 = dbParking.GetCollection<Billing>("Billing");
                    var filter2 = Builders<Billing>.Filter.Eq("_id", 1);
                    var document2 = collection2.Find(filter2).First();
                    float price = 0;
                    switch (diff.TotalMinutes)
                    {
                        case <= 15:
                            price = 0;
                            break;
                        case <= 30:
                            price = document2.halfanhour;
                            break;
                        case <= 60:
                            price = document2.onehour;
                            break;
                        case <= 180:
                            price = document2.threehours;
                            break;
                        case <= 360:
                            price = document2.sixhours;
                            break;
                        case <= 1440:
                            price = Convert.ToInt32(diff.TotalDays) * document2.daily;
                            break;
                        default:
                            break;
                    }
                    filter = Builders<Ticket>.Filter.Eq("_id", IDTicket);
                    var update = Builders<Ticket>.Update.Set("PaymentTime", paymentime).Set("Bill", price + document.Bill);
                    collection.UpdateOne(filter, update);
                    res = new { Status = true, IstantBill = price, TotalBill = price + document.Bill };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    res = new { Status = false, IstantBill = -1, TotalBill = -1 };
                }
                return res;
            });
        }
        public Task<bool> Exit(string IDTicket, DateTime exitime)
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParking.GetCollection<Ticket>("Ticket");
                    //calcolo quanto tempo è passato da quando il cliente ha pagato a quando è arrivato alla sbarra
                    var filter = Builders<Ticket>.Filter.Eq("_id", IDTicket);
                    var document = collection.Find(filter).First();
                    System.TimeSpan diff = exitime.Subtract(document.PaymentTime);
                    //se sono passati meno di 15 minuti dal pagamento il cliente può uscire

                    if (diff.TotalMinutes <= 15)
                    {
                        Console.WriteLine("sotto i 15 min");
                        filter = Builders<Ticket>.Filter.Eq("_id", IDTicket);
                        var update = Builders<Ticket>.Update.Set("ExitTime", exitime);
                        var queryResult = collection.UpdateOne(filter, update);
                        //non è necessario scrivere l'uscita visto che viene cancellata subito
                        //DeleteRecordTicket(IDTicket);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
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
                    var collection = dbParking.GetCollection<Ticket>("Ticket");
                    var filter = Builders<Ticket>.Filter.Eq("_id", IDTicket);
                    var document = collection.Find(filter).First();
                    return (float)document.Bill;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return -1;
                }
            });
        }
        public Task UpdateBilling(Billing myBill)
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParking.GetCollection<Billing>("Billing");
                    var filter = Builders<Billing>.Filter.Eq("id", 1);
                    var update = Builders<Billing>.Update.Set("halfanhour", myBill.halfanhour)
                                                    .Set("onehour", myBill.onehour)
                                                    .Set("threehours", myBill.threehours)
                                                    .Set("sixhours", myBill.sixhours)
                                                    .Set("daily", myBill.daily);
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
                    var collection = dbParking.GetCollection<Ticket>("Ticket");
                    var deleteFilter = Builders<Ticket>.Filter.Eq("_id", IDTicket);
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
        //Aggiorna lo stato e la data di una piazzola specifica
        public Task UpdateParkingSpot(string ID, Boolean Status, DateTime time)
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParking.GetCollection<ParkingSpot>("ParkingSpot");
                    var filter = Builders<ParkingSpot>.Filter.Eq("_id", ID);
                    var update = Builders<ParkingSpot>.Update.Set("Status", Status).Set("Timestamp", time);
                    collection.UpdateOne(filter, update);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
        public Task<int> GetBusyParkingSpot()
        {
            return Task.Run(() =>
            {
                var collection = dbParking.GetCollection<ParkingSpot>("ParkingSpot");
                var filter = Builders<ParkingSpot>.Filter.Eq("Status", true);
                var num = (int) collection.Find(filter).Count();
                return num;
            });
        }
        //Metodi per popolare da zero il db Mongo
        public Task CreateBilling()
        {
            return Task.Run(() =>
            {
                var collection = dbParking.GetCollection<Billing>("Billing");
                Billing myBilling;
                myBilling = new()
                {
                    id = 1,
                    halfanhour = 0,
                    onehour = 0,
                    threehours = 0,
                    sixhours = 0,
                    daily = 0
                };
                collection.InsertOne(myBilling);
            });
        }
        public Task CreateParkingSpot()
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParking.GetCollection<ParkingSpot>("ParkingSpot");
                    if (collection.Count(_ => true) == 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 50; j++)
                            {
                                ParkingSpot myParkingSpot;
                                if (j < 10)
                                {
                                    myParkingSpot = new()
                                    {
                                        Id = i + "0" + j,
                                        IdFloor = i,
                                        IdParkingSpot = j
                                    };
                                }
                                else
                                {
                                    myParkingSpot = new()
                                    {
                                        Id = i + "" + j,
                                        IdFloor = i,
                                        IdParkingSpot = j
                                    };
                                }
                                collection.InsertOne(myParkingSpot);
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
        public Task DropTableParkingSpot()
        {
            return Task.Run(() =>
            {
                try
                {
                    var collection = dbParking.GetCollection<ParkingSpot>("ParkingSpot");
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 50; j++)
                        {
                            if (j < 10)
                            {
                                var deleteFilter = Builders<ParkingSpot>.Filter.Eq("_id", i + "0" + j);
                                collection.DeleteOne(deleteFilter);
                            }
                            else
                            {
                                var deleteFilter = Builders<ParkingSpot>.Filter.Eq("_id", i + "" + j);
                                collection.DeleteOne(deleteFilter);
                            }
                        }
                    }
                    Console.WriteLine("finito eliminazione");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }
    }
}