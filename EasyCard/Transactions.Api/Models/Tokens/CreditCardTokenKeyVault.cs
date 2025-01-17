﻿using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Tokens
{
    /// <summary>
    /// Entity to store in KeyVault
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreditCardTokenKeyVault : CreditCardDetailsBase
    {
        public Guid? TerminalID { get; set; }

        public Guid? MerchantID { get; set; }

        /// <summary>
        /// Reference to initial transaction
        /// </summary>
        public Guid? InitialTransactionID { get; set; }

        ///// <summary>
        ///// Authorization code
        ///// </summary>
        //public string OKNumber { get; set; }
    }
}
