using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Swagger;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Billing
{
    public class FutureBillingDealsFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        [SwaggerExclude]
        public Guid? MerchantID { get; set; }

        public Guid? BillingDealID { get; set; }

        public CurrencyEnum? Currency { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        public string CardNumber { get; set; }

        public string CardOwnerNationalID { get; set; }

        public string CardOwnerName { get; set; }

        [SwaggerExclude]
        public string CreditCardVendor { get; set; }
    }
}
