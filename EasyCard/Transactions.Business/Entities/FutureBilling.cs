using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Transactions.Shared.Models;

namespace Transactions.Business.Entities
{
    public class FutureBilling
    {
        public FutureBilling()
        {
        }

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

        [JsonIgnore]
        public DateTime? CardExpirationDate { get; set; }

        public CardExpiration CardExpiration
        {
            get { return CreditCardHelpers.ParseCardExpiration(CardExpirationDate); }
            set { CardExpirationDate = value?.ToDate(); }
        }

        public DateTime? PausedFrom { get; set; }

        public DateTime? PausedTo { get; set; }

        public int? CurrentDeal { get; set; }

        public DateTime? NextScheduledTransaction { get; set; }

        public int? FutureDeal { get; set; }

        public DateTime? FutureTransactionDate { get; set; }
    }
}
