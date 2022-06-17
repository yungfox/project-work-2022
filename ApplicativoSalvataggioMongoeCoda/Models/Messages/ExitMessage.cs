using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda.Models.Messages
{
    public class ExitMessage
    {
        public string _id { get; set; }
        public DateTime exitTime { get; set; }
    }
}
