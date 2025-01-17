﻿using Newtonsoft.Json;
using Shared.Helpers;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Shared.Helpers.Models;

namespace Shared.Integration.Models.Invoicing
{
    public class InvoicingCreateDocumentRequest
    {
        public InvoicingCreateDocumentRequest()
        {
            this.DealDetails = new DealDetails();
        }

        public string InvoiceID { get; set; }

        public object InvoiceingSettings { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Deal information (optional)
        /// </summary>
        public DealDetails DealDetails { get; set; }

        public IEnumerable<PaymentDetails.PaymentDetails> PaymentDetails { get; set; }

        /// <summary>
        /// Invoice amount (should be omitted in case of installment deal)
        /// </summary>
        public decimal? InvoiceAmount { get; set; }

        public decimal VATRate { get; set; }

        public decimal VATTotal { get; set; }

        public decimal NetTotal { get; set; }

        public decimal? TotalDiscount { get; set; }

        public string ConsumerName { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [IsraelNationalIDValidator]
        public string ConsumerNationalID { get; set; }

        /// <summary>
        /// Number Of payments (cannot be more than 999)
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Initial installment payment
        /// </summary>
        public decimal InitialPaymentAmount { get; set; }

        /// <summary>
        /// TotalAmount = InitialPaymentAmount + (NumberOfInstallments - 1) * InstallmentPaymentAmount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Amount of one instalment payment
        /// </summary>
        public decimal InstallmentPaymentAmount { get; set; }

        /// <summary>
        /// Generic transaction type
        /// </summary>
        public TransactionTypeEnum? TransactionType { get; set; }

        public Newtonsoft.Json.Linq.JObject Extension { get; set; }
    }
}
