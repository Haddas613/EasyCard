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
    // NOTE: this is J2 request

    /// <summary>
    /// Check if credit card is valid
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CheckCreditCardRequest : TransactionRequestBase
    {
        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Is the card physically scanned (telephone deal or magnetic)
        /// </summary>
        [EnumDataType(typeof(Enums.CardPresenceEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.CardPresenceEnum CardPresence { get; set; }

        /// <summary>
        /// Credit card details
        /// </summary>
        [Required]
        public CreditCardSecureDetails CreditCardSecureDetails { get; set; }
    }
}