using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Enums = Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Transactions
{
    //NOTE: this is J4 deal

    /// <summary>
    /// Create the charge based on credit card or previously stored credit card token
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreateTransactionRequest : TransactionRequestBase
    {
        /// <summary>
        /// Generic transaction type
        /// </summary>
        [EnumDataType(typeof(Enums.TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.TransactionTypeEnum TransactionType { get; set; }

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
        [StringLength(50)]
        public string CreditCardToken { get; set; }

        /// <summary>
        /// Transaction amount (should be omitted in case of installment deal)
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? TransactionAmount { get; set; }

        /// <summary>
        /// Installment payments details (should be omitted in case of regular deal)
        /// </summary>
        public InstallmentDetails InstallmentDetails { get; set; }

        /// <summary>
        /// Original consumer IP
        /// </summary>
        [StringLength(32)]
        public string ConsumerIP { get; set; }
    }
}
