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
        [BsonRepresentation(BsonType.ObjectId)]
        public int Id { get; set; }
        public int IdPiano { get; set; }
        public int IdPiazzola { get; set; }
        public DateTime Orario { get; set; }
        public Boolean Stato { get; set; }
        https://www.mongodb.com/blog/post/quick-start-c-sharp-and-mongodb-creating-documents
    }
}
