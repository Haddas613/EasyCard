using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Billing
{
    internal class BillingDealQueueEntry
    {
        public Guid BillingDealID { get; set; }

        public Guid TerminalID { get; set; }
    }
}
