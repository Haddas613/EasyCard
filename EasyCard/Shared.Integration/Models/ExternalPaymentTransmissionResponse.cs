using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ExternalPaymentTransmissionResponse
    {
        public string BadTrans { get; set; }

        public string RefNumber { get; set; }

        public string Report { get; set; }

        public string ErrorMessage { get; set; }

        public int ProcessorCode { get; set; }
        public bool Success { get; set; }
        public long TotalCreditTransSum { get; set; }
        public long TotalDebitTransSum { get; set; }
        public string TotalXML { get; set; }

    }
}
