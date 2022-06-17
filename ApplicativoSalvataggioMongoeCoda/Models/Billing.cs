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
        public float halfanhour { get; set; }
        public float onehour { get; set; }
        public float threehours { get; set; }
        public float sixhours { get; set; }
        public float daily { get; set; }
    }
}
