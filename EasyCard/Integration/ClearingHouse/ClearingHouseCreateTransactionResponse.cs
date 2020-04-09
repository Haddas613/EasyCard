using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearingHouse
{
    public class ClearingHouseCreateTransactionResponse : AggregatorCreateTransactionResponse
    {
        public long? ClearingHouseTransactionID { get; set; }

        public string ConcurrencyToken { get; set; }

        public string CorrelationID { get; set; }
    }
}
