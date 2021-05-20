using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Billing
{
    public class CreateTransactionFromBillingDealsResponse : OperationResponse
    {
        public int SuccessfulCount { get; set; }

        public int FailedCount { get; set; }
    }
}
