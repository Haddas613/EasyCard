﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Transactions.Shared.Models;

namespace Transactions.Business.Entities
{
    public class FutureBilling
    {
        public FutureBilling()
        {
            //CreditCardDetails = new CreditCardDetails();
        }

        public Guid BillingDealID { get; set; }

        public Guid TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public decimal TransactionAmount { get; set; }

        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public DateTime? BillingDealTimestamp { get; set; }

        public DateTime? NextScheduledTransaction { get; set; }

        public DateTime? FutureScheduledTransaction { get; set; }

        public string CardOwnerName { get; set; }

        /// <summary>
        /// Billing Schedule
        /// </summary>
        //public BillingSchedule BillingSchedule { get; set; }

        public string CardNumber { get; set; }

        /// <summary>
        /// Credit card information(just to display)
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; set; }

        public bool Active { get; set; }

        public int? CurrentDeal { get; set; }

        public DateTime? PausedFrom { get; set; }

        public DateTime? PausedTo { get; set; }
    }
}