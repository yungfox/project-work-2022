using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda.Models
{
    class ParkingSpot
    {
        [BsonId]
        public string Id { get; set; }
        public int IdPFloor { get; set; }
        public int IdParkingSpot { get; set; }
        public DateTime Timestamp { get; set; }
        public Boolean Status { get; set; }
    }
}
