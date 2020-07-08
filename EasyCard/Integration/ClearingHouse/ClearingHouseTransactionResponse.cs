using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearingHouse
{
    public class ClearingHouseTransactionResponse : AggregatorTransactionResponse
    {
        public long? ClearingHouseTransactionID { get; set; }

        public string ConcurrencyToken { get; set; }
    }
}
