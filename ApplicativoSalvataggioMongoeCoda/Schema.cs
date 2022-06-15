using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApplicativoSalvataggioMongoeCoda
{
    class Schema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String IdBiglietto { get; set; }
        public DateTime OrarioEntrata { get; set; }
        public DateTime OrarioPagamento { get; set; }
        public DateTime OrarioUscita { get; set; }
        public Double Prezzo { get; set; }
    }
}