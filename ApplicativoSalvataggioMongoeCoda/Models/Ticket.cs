using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApplicativoSalvataggioMongoeCoda.Models
{
    class Ticket
    {
        [BsonId]
        public string IdTicket { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime PaymentTime { get; set; }
        public DateTime ExitTime { get; set; }
        public double Bill { get; set; }
    }
}