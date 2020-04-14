using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearingHouse
{
    public class ClearingHouseCancelTransactionResponse : AggregatorCancelTransactionResponse
    {
        public string CorrelationID { get; set; }
    }
}
