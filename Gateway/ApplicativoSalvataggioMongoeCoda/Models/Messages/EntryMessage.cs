using System;

namespace ApplicativoSalvataggioMongoeCoda.Models.Messages
{
    internal class EntryMessage
    {
        public string _id { get; set; }
        public DateTime entryTime { get; set; }
    }
}
