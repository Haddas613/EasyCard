using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva
{
    public class ShvaTransmissionResponse : ProcessorTransmitTransactionsResponse
    {
        // TODO: please add description
        public string Report { get; set; }

        // TODO: codes enum
        public int? ProcessorCode { get; set; }

        public decimal? TotalCreditTransSum { get; set; }

        public decimal? TotalDebitTransSum { get; set; }

        // TODO: what s this fiels, how we can use this information?
        public string TotalXML { get; set; }
    }
}
