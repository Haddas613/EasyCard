using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class CreditCardDetails
    {
        /// <summary>
        /// Do not store full card number. Please use 123456****1234 pattern CreditCardHelpers.GetCardDigits()
        /// </summary>
        public string CardNumber { get; set; }

        public CardExpiration CardExpiration { get; set; }

        public string CardBin { get; set; }

        public string CardVendor { get; set; }

        public string CardOwnerNationalId { get; set; }

        public string CardOwnerName { get; set; }

        public string CardToken { get; set; }
    }
}
