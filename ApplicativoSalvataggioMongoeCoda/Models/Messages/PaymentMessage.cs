using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda.Models.Messages
{
    public class PaymentMessage
    {
        public string _id { get; set; }
        public DateTime paymentTime { get; set; }
        public float bill { get; set; }
    }
}
