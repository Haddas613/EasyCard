﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Enums = Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Transactions
{
    /// <summary>
    /// Billing deal
    /// </summary>
    public class NextBillingDealRequest : TransactionRequestBase
    {
        /// <summary>
        /// Initial billing deal id
        /// </summary>
        [Required]
        public Guid BillingDealID { get; set; }

        /// <summary>
        /// Refund amount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        [Required]
        public decimal TransactionAmount { get; set; }
    }
}
