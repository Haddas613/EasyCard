using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Api.Swagger;
using Shared.Helpers;
using Shared.Helpers.Models;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Transactions.Shared.Enums;
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
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Generic transaction type
        /// </summary>
        [EnumDataType(typeof(TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Payment Type
        /// </summary>
        [EnumDataType(typeof(PaymentTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentTypeEnum PaymentTypeEnum { get; set; }

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
        /// Bank transfer details. Should be omitted in case of card transaction
        /// </summary>
        public BankTransferDetails BankTransferDetails { get; set; }

        /// <summary>
        /// Stored credit card details token (should be omitted in case if full credit card details used)
        /// </summary>
        public Guid? CreditCardToken { get; set; }

        /// <summary>
        /// Transaction amount. Must always be specified. In case of Installments must match InstallmentDetails.TotalAmount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal TransactionAmount { get; set; }

        [Range(0, 1)]
        [DataType(DataType.Currency)]
        public decimal? VATRate { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? VATTotal { get; set; }

        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? NetTotal { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? NetDiscountTotal { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? DiscountTotal { get; set; }

        /// <summary>
        /// Installment payments details (should be omitted in case of regular deal)
        /// </summary>
        public InstallmentDetails InstallmentDetails { get; set; }

        /// <summary>
        /// Original consumer IP
        /// </summary>
        [StringLength(32)]
        public string ConsumerIP { get; set; }

        /// <summary>
        /// Save credit card from request.
        /// Requires Feature CreditCardTokens to be enabled.
        /// </summary>
        public bool? SaveCreditCard { get; set; }

        /// <summary>
        /// Reference to initial transaction
        /// </summary>
        public Guid? InitialJ5TransactionID { get; set; }

        /// <summary>
        /// Create document
        /// </summary>
        public bool? IssueInvoice { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// Create Pinpad Transaction
        /// </summary>
        public bool? PinPad { get; set; }

        /// <summary>
        ///  Pinpad device in case of terminal with multiple devices
        /// </summary>
        public string PinPadDeviceID { get; set; }

        public Guid? PaymentRequestID { get; set; }

        [SwaggerExclude]
        public Guid? PaymentIntentID { get; set; }

        // ShvaAuthNum
        public string OKNumber { get; set; }

        //public void Calculate()
        //{
        //    if (NetTotal.GetValueOrDefault(0) == 0)
        //    {
        //        NetTotal = Math.Round(TransactionAmount / (1m + VATRate.GetValueOrDefault(0)), 2, MidpointRounding.AwayFromZero);
        //        VATTotal = TransactionAmount - NetTotal;
        //    }
        //}

        //public void Calculate(decimal vatRate)
        //{
        //    VATRate = vatRate;
        //    NetTotal = Math.Round(TransactionAmount / (1m + VATRate.GetValueOrDefault(0)), 2, MidpointRounding.AwayFromZero);
        //    VATTotal = TransactionAmount - NetTotal;
        //}

        /// <summary>
        /// Only to be used for pin pad transactions when CreditCardSecureDetails is not available
        /// </summary>
        [SwaggerExclude]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [IsraelNationalIDValidator]
        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// Only to be used for pin pad transactions when CreditCardSecureDetails is not available
        /// </summary>
        [StringLength(50, MinimumLength = 2)]
        [SwaggerExclude]
        public string CardOwnerName { get; set; }

        /// <summary>
        /// SignalR connection id
        /// </summary>
        [SwaggerExclude]
        public string ConnectionID { get; set; }

        [SwaggerExclude]
        public JObject Extension { get; set; }

        [SwaggerExclude]
        [StringLength(50)]
        public string ThreeDSServerTransID { get; set; }

        [StringLength(50)]
        public string Origin { get; set; }

        public bool UserAmount { get; set; }
    }
}
