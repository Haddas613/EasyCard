using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Api.Swagger;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Enums = Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Transactions
{
    // NOTE: this is J5 request

    /// <summary>
    /// Blocking funds on credit card
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BlockCreditCardRequest : TransactionRequestBase
    {
        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Is the card physically scanned (telephone deal or magnetic)
        /// </summary>
        [EnumDataType(typeof(CardPresenceEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardPresenceEnum CardPresence { get; set; }

        /// <summary>
        /// Credit card details (should be omitted in case if stored credit card token used)
        /// </summary>
        public CreditCardSecureDetails CreditCardSecureDetails { get; set; }

        /// <summary>
        /// Stored credit card details token (should be omitted in case if full credit card details used)
        /// </summary>
        public Guid? CreditCardToken { get; set; }

        /// <summary>
        /// Transaction amount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        [Required]
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Save credit card from request.
        /// Requires Feature CreditCardTokens to be enabled.
        /// </summary>
        public bool? SaveCreditCard { get; set; }

        public Guid? PaymentRequestID { get; set; }

        [SwaggerExclude]
        public Guid? PaymentIntentID { get; set; }

        [SwaggerExclude]
        public JObject Extension { get; set; }

        [SwaggerExclude]
        [StringLength(50)]
        public string ThreeDSServerTransID { get; set; }

        [StringLength(50)]
        public string Origin { get; set; }
    }
}
