using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda.Models
{
    class Billing
    {
        [BsonId]
        public int id { get; set; }
        public float onehour { get; set; }
    }
}
