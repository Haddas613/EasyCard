using Shared.Api.Models;
using Shared.Api.Swagger;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions.Enums;

namespace Transactions.Api.Models.Billing
{
    public class BillingDealsFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        [SwaggerExclude]
        public Guid? MerchantID { get; set; }

        public Guid? BillingDealID { get; set; }

        public CurrencyEnum? Currency { get; set; }

        public QuickDateFilterTypeEnum? QuickDateFilter { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public DateFilterTypeEnum DateType { get; set; }

        public Guid? ConsumerID { get; set; }

        public Guid? CreditCardTokenID { get; set; }

        public string CardNumber { get; set; }

        public string CardOwnerNationalID { get; set; }

        public string CardOwnerName { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        [SwaggerExclude]
        public string CreditCardVendor { get; set; }
    }
}
