using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda
{
    class Piazzola
    {
        [BsonId]
        public int Id { get; set; }
        public int IdPiano { get; set; }
        public int IdPiazzola { get; set; }
        public DateTime Orario { get; set; }
        public Boolean Stato { get; set; }
    }
}
