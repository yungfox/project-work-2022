using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda.Models.Messages
{
    public class ParkingSpotMessage
    {
        public string _id { get; set; }
        public bool taken { get; set; }
        public DateTime timestamp { get; set; }
    }
}
