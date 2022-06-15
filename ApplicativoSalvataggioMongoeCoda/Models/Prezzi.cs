using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda.Models
{
    class Prezzi
    {
        [BsonId]
        public int id { get; set; }
        public float mezzora { get; set; }
        public float unora { get; set; }
        public float treore { get; set; }
        public float seiore { get; set; }
        public float giornaliero { get; set; }
    }
}
