using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Enums = Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Transactions
{
    // NOTE: this is J4 request

    /// <summary>
    /// Refund request
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class RefundRequest : TransactionRequestBase
    {
        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Is the card physically scanned (telephone deal or magnetic)
        /// </summary>
        [EnumDataType(typeof(Enums.CardPresenceEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.CardPresenceEnum CardPresence { get; set; }

        /// <summary>
        /// Credit card details (should be omitted in case if stored credit card token used)
        /// </summary>
        public CreditCardSecureDetails CreditCardSecureDetails { get; set; }

        /// <summary>
        /// Stored credit card details token (should be omitted in case if full credit card details used)
        /// </summary>
        public string CreditCardToken { get; set; }

        /// <summary>
        /// Refund amount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        [Required]
        public decimal TransactionAmount { get; set; }
    }
}
