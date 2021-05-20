using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Billing
{
    public class CreateTransactionFromBillingDealsRequest
    {
        public IEnumerable<Guid> BillingDealsID { get; set; }
    }
}
