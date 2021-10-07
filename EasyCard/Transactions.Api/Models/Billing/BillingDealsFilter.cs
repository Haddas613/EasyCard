using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Swagger;
using Shared.Helpers;
using Shared.Integration.Models;
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

        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

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

        /// <summary>
        /// Billing deals that can be manually triggered
        /// </summary>
        public bool Actual { get; set; }

        public bool Finished { get; set; }

        public bool Paused { get; set; }

        public bool HasError { get; set; }

        public PaymentTypeEnum? PaymentType { get; set; }

        /// <summary>
        /// Merchant deal reference
        /// </summary>
        public string DealReference { get; set; }
    }
}
