using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;

namespace Transactions.Api.Models.Billing
{
    public class FutureBillingSummary
    {
        public Guid BillingDealID { get; set; }

        public DateTime? BillingDealTimestamp { get; set; }

        public Guid TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public Guid? ConsumerID { get; set; }

        public decimal TransactionAmount { get; set; }

        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public string CardOwnerName { get; set; }

        public string CardNumber { get; set; }

        public CardExpiration CardExpiration { get; set; }

        public DateTime? PausedFrom { get; set; }

        public DateTime? PausedTo { get; set; }

        public int? CurrentDeal { get; set; }

        public DateTime? NextScheduledTransaction { get; set; }

        public int? FutureDeal { get; set; }

        public DateTime? FutureTransactionDate { get; set; }
    }
}
