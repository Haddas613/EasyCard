using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.PaymentRequests
{
    /// <summary>
    /// Information regarding what user actually paid in payment request (only relevant for UserAmount allowed PRs)
    /// </summary>
    public class PaymentRequestUserPaidDetails
    {
        public decimal VATRate { get; set; }

        public decimal VATTotal { get; set; }

        public decimal NetTotal { get; set; }

        public decimal TransactionAmount { get; set; }
    }
}
