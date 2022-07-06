using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
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
        [MetadataOptions(Hidden = true)]
        public Guid BillingDealID { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid TerminalID { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? ConsumerID { get; set; }

        public decimal TransactionAmount { get; set; }

        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public string CardOwnerName { get; set; }

        public string CardNumber { get; set; }

        [MetadataOptions(Hidden = true)]
        public CardExpiration CardExpiration { get; set; }

        [MetadataOptions(Hidden = true)]
        public DateTime? PausedFrom { get; set; }

        [MetadataOptions(Hidden = true)]
        public DateTime? PausedTo { get; set; }

        public int? CurrentDeal { get; set; }

        [MetadataOptions(Hidden = true)]
        public DateTime? NextScheduledTransaction { get; set; }

        public int? FutureDeal { get; set; }

        public DateTime? FutureTransactionDate { get; set; }
    }
}
