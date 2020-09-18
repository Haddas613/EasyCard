using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;
using Transactions.Shared.Models;

namespace Transactions.Api.Models.Billing
{
    public class BillingDealSummary
    {
        public Guid BillingDealID { get; set; }

        public Guid TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Number Of payments (cannot be more than 999)
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// TotalAmount = TransactionAmount * NumberOfPayments
        /// </summary>
        public decimal TotalAmount { get; set; }

        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public DateTime? BillingDealTimestamp { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        [EnumDataType(typeof(BillingDealStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public BillingDealStatusEnum Status { get; set; }

        public string CardOwnerName { get; set; }

        /// <summary>
        /// Billing Schedule
        /// </summary>
        public BillingSchedule BillingSchedule { get; set; }

        public string CardNumber { get; set; }
    }
}
