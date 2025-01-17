﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Api.Models.Binding;
using Shared.Api.Swagger;
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
    /// <summary>
    /// Create a link to Checkout Page
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PaymentRequestCreate
    {
        /// <summary>
        /// EasyCard Terminal
        /// </summary>
        [SwaggerExclude]
        public Guid? TerminalID { get; set; }

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

        /// <summary>
        /// Deal amount including VAT. This amount will be displayed on Checkout Page. Consumer can override this amount in case if UserAmount flag specified.
        /// </summary>
        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? PaymentRequestAmount { get; set; }

        /// <summary>
        /// Due date of payment link
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Transaction Type
        /// </summary>
        public TransactionTypeEnum? TransactionType { get; set; }

        /// <summary>
        /// Installment payments details (should be omitted in case of regular deal)
        /// </summary>
        public InstallmentDetails InstallmentDetails { get; set; }

        /// <summary>
        /// Invoice details. In case if omitted default values will be used. Ignored in case if "IssueInvoice" is false
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// Pinpad device details
        /// </summary>
        public PinPadDetails PinPadDetails { get; set; }

        /// <summary>
        /// Create document - Invoice, Receipt etc. If omitted, default terminal settings will be used
        /// </summary>
        public bool? IssueInvoice { get; set; }

        /// <summary>
        /// Enables PinPad button on checkout page
        /// </summary>
        public bool? AllowPinPad { get; set; }

        /// <summary>
        /// Deal tax rate. Can be omitted if only PaymentRequestAmount specified - in this case VAT rate from terminal settings will be used
        /// </summary>
        [Range(0, 1)]
        [DataType(DataType.Currency)]
        public decimal? VATRate { get; set; }

        /// <summary>
        /// Total deal tax amount. VATTotal = NetTotal * VATRate. Can be omitted if only PaymentRequestAmount specified
        /// </summary>
        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? VATTotal { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? NetDiscountTotal { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? DiscountTotal { get; set; }

        /// <summary>
        /// Deal amount before tax. PaymentRequestAmount = NetTotal + VATTotal. Can be omitted if only PaymentRequestAmount specified
        /// </summary>
        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? NetTotal { get; set; }

        /// <summary>
        /// You can override default email subject When sending payment link via email
        /// </summary>
        [StringLength(250)]
        [SwaggerExclude]
        public string RequestSubject { get; set; }

        /// <summary>
        /// You can override "from" address subject When sending payment link via email
        /// </summary>
        [StringLength(100)]
        [SwaggerExclude]
        public string FromAddress { get; set; }

        /// <summary>
        /// Generate link to Checkout Page to create refund
        /// </summary>
        public bool IsRefund { get; set; }

        /// <summary>
        /// Url to merchant's web site. Base url should be configured in terminal settings. You can add any details to query string.
        /// </summary>
        [StringLength(1000)]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Consumer can override PaymentRequestAmount
        /// </summary>
        public bool UserAmount { get; set; }

        [SwaggerExclude]
        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// Any advanced payload which will be stored in EasyCard and then can be obtained using "GetTransaction"
        /// </summary>
        public JObject Extension { get; set; }

        // TODO: can be used for email template

        /// <summary>
        /// Default language to display checkout page
        /// </summary>
        public string Language { get; set; }

        [StringLength(50)]
        public string Origin { get; set; }

        public bool? AllowRegular { get; set; }

        public bool? AllowInstallments { get; set; }

        public bool? AllowCredit { get; set; }

        public bool? AllowImmediate { get; set; }

        public bool? HidePhone { get; set; }

        public bool? PhoneRequired { get; set; }

        public bool? HideEmail { get; set; }

        public bool? EmailRequired { get; set; }

        public bool? HideNationalID { get; set; }

        public bool? NationalIDRequired { get; set; }

        public bool? ShowAuthCode { get; set; }

        public bool? ConsumerDataReadonly { get; set; }

        /// <summary>
        /// J3, J4, J5
        /// </summary>
        public JDealTypeEnum JDealType { get; set; }

        public bool? SaveCreditCardByDefault { get; set; }

        public bool? DisableCancelPayment { get; set; }
    }
}
