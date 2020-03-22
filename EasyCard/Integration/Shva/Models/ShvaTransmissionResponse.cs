using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    public class ShvaTransmissionResponse
    {
        // TODO: array with transaction ids ?
        public string BadTrans { get; set; }

        // TODO: please add description
        public string RefNumber { get; set; }

        // TODO: please add description
        public string Report { get; set; }

        public string ErrorMessage { get; set; }

        // TODO: codes enum
        public int ProcessorCode { get; set; }

        public bool Success { get; set; }

        public decimal TotalCreditTransSum { get; set; }

        public decimal TotalDebitTransSum { get; set; }

        // TODO: what s this fiels, how we can use this information?
        public string TotalXML { get; set; }
    }
}
