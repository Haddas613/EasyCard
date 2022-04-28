using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Models;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Models.Billing
{
    public class BillingDealSummaryAdmin : BillingDealSummary
    {
        public new Guid BillingDealID { get; set; }

        public string MerchantName { get; set; }

        [MetadataOptions(Hidden = true)]
        [ExcelIgnore]
        public new Guid MerchantID { get; set; }

        [MetadataOptions(Hidden = true)]
        [ExcelIgnore]
        public new Guid TerminalID { get; set; }

        public new BillingSchedule BillingSchedule { get; set; }

        [MetadataOptions(Hidden = true)]
        [ExcelIgnore]
        public new SharedIntegration.Models.DealDetails DealDetails { get; set; }

        /// <summary>
        /// Stored credit card details token
        /// </summary>
        public new Guid? CreditCardToken { get; set; }
    }
}
