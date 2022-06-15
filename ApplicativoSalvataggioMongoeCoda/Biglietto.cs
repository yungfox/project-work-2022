using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApplicativoSalvataggioMongoeCoda
{
    class Biglietto
    {
        [BsonId]
        public string IdBiglietto { get; set; }
        public DateTime OrarioEntrata { get; set; }
        public DateTime OrarioPagamento { get; set; }
        public DateTime OrarioUscita { get; set; }
        public Double Prezzo { get; set; }
    }
}