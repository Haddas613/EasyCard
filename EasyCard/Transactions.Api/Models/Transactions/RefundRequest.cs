﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using Shared.Helpers.Models;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Api.Models.Binding;
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
        [StringLength(50)]
        public string CreditCardToken { get; set; }

        /// <summary>
        /// Refund amount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        [Required]
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

        /// <summary>
        /// Create document
        /// </summary>
        public bool? IssueInvoice { get; set; }

        /// <summary>
        /// Create Pinpad Transaction
        /// </summary>
        public bool? PinPad { get; set; }

        /// <summary>
        ///  Pinpad device in case of terminal with multiple devices
        /// </summary>
        public string PinPadDeviceID { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        public InstallmentDetails InstallmentDetails { get; set; }

        /// <summary>
        /// Generic transaction type
        /// </summary>
        [EnumDataType(typeof(TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// Only to be used for pin pad transactions when CredotCardSecureDetails is not available
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [IsraelNationalIDValidator]
        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// SignalR connection id
        /// </summary>
        public string ConnectionID { get; set; }

        /// <summary>
        /// ShvaAuthNum
        /// </summary>
        public string OKNumber { get; set; }
    }
}
