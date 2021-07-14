using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.Models.Binding;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.Processor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.PaymentRequests
{
    public class PaymentRequestCreate
    {
        /// <summary>
        /// Terminal
        /// </summary>
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Deal information (optional)
        /// </summary>
        public IntegrationModels.DealDetails DealDetails { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? PaymentRequestAmount { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Installment payments details (should be omitted in case of regular deal)
        /// </summary>
        public InstallmentDetails InstallmentDetails { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// pinpad details
        /// </summary>
        public PinPadDetails PinPadDetails { get; set; }

        /// <summary>
        /// Create document
        /// </summary>
        public bool? IssueInvoice { get; set; }

        /// <summary>
        /// Allow PinPad Deal
        /// </summary>
        public bool? AllowPinPad { get; set; }

        [Range(0, 1)]
        [DataType(DataType.Currency)]
        public decimal VATRate { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal VATTotal { get; set; }

        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal NetTotal { get; set; }

        /// <summary>
        /// Email subject
        /// </summary>
        [StringLength(250)]
        public string RequestSubject { get; set; }

        [StringLength(100)]
        public string FromAddress { get; set; }

        /// <summary>
        /// End-customer Name
        /// </summary>
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ConsumerName { get; set; }

        public bool IsRefund { get; set; }
    }
}
