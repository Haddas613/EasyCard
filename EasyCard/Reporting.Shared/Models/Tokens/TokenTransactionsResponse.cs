using Newtonsoft.Json;
using Shared.Api.UI;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Shared.Models.Tokens
{
    public class TokenTransactionsResponse
    {
        public Guid? CreditCardTokenID { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? TerminalID { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? MerchantID { get; set; }

        public string TerminalName { get; set; }

        public string MerchantName { get; set; }

        public string CardNumber { get; set; }

        [JsonConverter(typeof(CardExpirationConverter))]
        [MetadataOptions(Hidden = true)]
        public CardExpiration CardExpiration { get; set; }

        public string CardVendor { get; set; }

        public DateTime? Created { get; set; }

        public string CardOwnerName { get; set; }

        /// <summary>
        /// Consumer ID
        /// </summary>
        [MetadataOptions(Hidden = true)]
        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        public bool Expired { get; set; }

        public int ProductionTransactions { get; set; }

        public int FailedTransactions { get; set; }

        public decimal TotalSum { get; set; }

        public decimal TotalRefund { get; set; }
    }
}
