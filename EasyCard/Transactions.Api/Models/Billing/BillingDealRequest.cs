﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Shared.Models;

namespace Transactions.Api.Models.Billing
{
    public class BillingDealRequest : TransactionRequestBase
    {
        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Stored credit card details token (should be omitted in case if full credit card details used)
        /// </summary>
        public Guid CreditCardToken { get; set; }

        /// <summary>
        /// Transaction amount (should be omitted in case of installment deal)
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? TransactionAmount { get; set; }

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
        /// Billing Schedule
        /// </summary>
        public BillingSchedule BillingSchedule { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }
    }
}
