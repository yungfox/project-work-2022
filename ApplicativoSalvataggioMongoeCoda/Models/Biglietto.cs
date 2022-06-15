using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApplicativoSalvataggioMongoeCoda.Models
{
    class Biglietto
    {
        [BsonId]
        public string IdBiglietto { get; set; }
        public DateTime OrarioEntrata { get; set; }
        public DateTime OrarioPagamento { get; set; }
        public DateTime OrarioUscita { get; set; }
        public double Prezzo { get; set; }
    }
}