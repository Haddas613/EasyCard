using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Invoicing
{
    [JsonSubtypes.KnownSubType(typeof(CreditCardPaymentDetails), PaymentTypeEnum.Card)]
    public class InvoiceSummary : InvoiceSummaryAmounts
    {
        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid InvoiceID { get; set; }

        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Date-time when deal created initially in UTC
        /// </summary>
        public DateTime? InvoiceTimestamp { get; set; }

        /// <summary>
        /// Legal invoice day
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        [EnumDataType(typeof(InvoiceTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceTypeEnum InvoiceType { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        [EnumDataType(typeof(InvoiceStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceStatusEnum Status { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public string CardOwnerName { get; set; }

        public Guid? ConsumerID { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public DocumentOriginEnum DocumentOrigin { get; set; }

        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
    }
}
