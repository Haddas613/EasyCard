﻿using Shared.Integration.Models;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ConsumerResponse
    {
        public Guid ConsumerID { get; set; }

        public Guid MerchantID { get; set; }

        public bool Active { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        public string ConsumerName { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        public string ConsumerPhone { get; set; }

        public Address ConsumerAddress { get; set; }

        public string ConsumerNationalID { get; set; }

        public DateTime? Created { get; set; }

        public string OperationDoneBy { get; set; }

        public string CorrelationId { get; set; }

        public string ExternalReference { get; set; }

        public string Origin { get; set; }

        public string BillingDesktopRefNumber { get; set; }

        public string ConsumerSecondPhone { get; set; }

        public string ConsumerNote { get; set; }

        public BankDetails BankDetails { get; set; }

        /// <summary>
        /// External ID inside https://woocommerce.com system
        /// </summary>
        public string WoocommerceID { get; set; }

        /// <summary>
        /// External ID inside https://www.ecwid.com system
        /// </summary>
        public string EcwidID { get; set; }
    }
}
