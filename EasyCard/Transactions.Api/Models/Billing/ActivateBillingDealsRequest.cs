using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Billing
{
    public class ActivateBillingDealsRequest
    {
        public IEnumerable<Guid> BillingDealsID { get; set; }
    }
}
