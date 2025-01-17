﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;
using Transactions.Shared.Models;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Models.Billing
{
    public class BillingDealSummary
    {
        [MetadataOptions(Hidden = true)]
        public Guid BillingDealID { get; set; }

        public string TerminalName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid TerminalID { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid MerchantID { get; set; }

        public decimal TransactionAmount { get; set; }

        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public CurrencyEnum Currency { get; set; }

        public DateTime? BillingDealTimestamp { get; set; }

        public DateTime? NextScheduledTransaction { get; set; }

        /// <summary>
        /// Date-time when last created initially in UTC
        /// </summary>
        public DateTime? CurrentTransactionTimestamp { get; set; }

        public string ConsumerName { get; set; }

        /// <summary>
        /// Billing Schedule
        /// </summary>
        public BillingSchedule BillingSchedule { get; set; }

        public string CardNumber { get; set; }

        public bool? CardExpired { get; set; }

        public bool Active { get; set; }

        public int? CurrentDeal { get; set; }

        [MetadataOptions(Hidden = true)]
        public DateTime? PausedFrom { get; set; }

        [MetadataOptions(Hidden = true)]
        public DateTime? PausedTo { get; set; }

        public bool Paused { get; set; }

        public PaymentTypeEnum PaymentType { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        [MetadataOptions(Hidden = true)]
        public SharedIntegration.Models.DealDetails DealDetails { get; set; }

        public bool InvoiceOnly { get; set; }

        /// <summary>
        /// Stored credit card details token
        /// </summary>
        [MetadataOptions(Hidden = true)]
        public Guid? CreditCardToken { get; set; }

        public string ConsumerExternalReference { get; set; }
    }
}
