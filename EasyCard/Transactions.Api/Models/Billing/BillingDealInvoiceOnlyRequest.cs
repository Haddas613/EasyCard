using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Shared.Models;

namespace Transactions.Api.Models.Billing
{
    public class BillingDealInvoiceOnlyRequest : TransactionRequestBase
    {
        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        public Guid? TerminalID { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Transaction amount
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

        /// <summary>
        /// Billing Schedule
        /// </summary>
        [Required]
        public BillingSchedule BillingSchedule { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        [Required]
        public InvoiceDetails InvoiceDetails { get; set; }

        public PaymentTypeEnum PaymentType { get; set; }

        /// <summary>
        /// Array of payment details, e.g. CreditCardDetails, ChequeDetails etc.
        /// </summary>
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }

        /// <summary>
        /// If VATRate, NetTotal and VatTotal properties were not specified, this method should be called
        /// </summary>
        public void Calculate(decimal vatRate)
        {
            //Only calculated if values are not present
            if (VATRate.HasValue)
            {
                return;
            }

            VATRate = vatRate;
            NetTotal = Math.Round(TransactionAmount / (1m + VATRate.Value), 2, MidpointRounding.AwayFromZero);
            VATTotal = TransactionAmount - NetTotal;
        }
    }
}
