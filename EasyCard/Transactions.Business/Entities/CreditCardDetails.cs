using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    /// <summary>
    /// Do not store full card number. Please use 123456****1234 pattern CreditCardHelpers.GetCardDigits()
    /// </summary>
    public class CreditCardDetails : CreditCardDetailsBase
    {
        public string CardOwnerName { get; set; }

        /// <summary>
        /// Stored token reference
        /// </summary>
        public string CardToken { get; set; }

        public bool? IsTourist { get; set; }
    }
}
