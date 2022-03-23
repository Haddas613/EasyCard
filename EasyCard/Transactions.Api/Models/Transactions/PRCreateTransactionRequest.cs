using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.Swagger;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
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
    /// <summary>
    /// Create the charge based on payment request
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PRCreateTransactionRequest
    {
        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

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
        /// Original consumer IP
        /// </summary>
        [StringLength(32)]
        public string ConsumerIP { get; set; }

        /// <summary>
        /// Save credit card from request
        /// </summary>
        public bool? SaveCreditCard { get; set; }

        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid? PaymentRequestID { get; set; }

        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid? PaymentIntentID { get; set; }

        public bool? IssueInvoice { get; set; }

        public bool? PinPad { get; set; }

        /// <summary>
        ///  Pinpad device in case of terminal with multiple devices
        /// </summary>
        public string PinPadDeviceID { get; set; }

        public InstallmentDetails InstallmentDetails { get; set; }

        /// <summary>
        /// Generic transaction type
        /// </summary>
        [EnumDataType(typeof(TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? PaymentRequestAmount { get; set; }

        [Range(0, 1)]
        [DataType(DataType.Currency)]
        public decimal? VATRate { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? VATTotal { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? NetTotal { get; set; }

        /// <summary>
        /// Only to be used for pin pad transactions when CreditCardSecureDetails is not available
        /// </summary>
        [StringLength(20)]
        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// Only to be used for pin pad transactions when CreditCardSecureDetails is not available
        /// </summary>
        [StringLength(50, MinimumLength = 2)]
        public string CardOwnerName { get; set; }

        /// <summary>
        /// SignalR connection ID
        /// </summary>
        public string ConnectionID { get; set; }

        public string OKNumber { get; set; }

        /// <summary>
        /// For Bit Processor only
        /// </summary>
        [SwaggerExclude]
        public string BitPaymentInitiationId { get; set; }

        /// <summary>
        /// For Bit Processor only
        /// </summary>
        [SwaggerExclude]
        public string BitTransactionSerialId { get; set; }

        [SwaggerExclude]
        [StringLength(50)]
        public string ThreeDSServerTransID { get; set; }
    }
}
