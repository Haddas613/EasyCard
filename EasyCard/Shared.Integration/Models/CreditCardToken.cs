using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    /// <summary>
    /// To be used only in Shva
    /// </summary>
    public class CreditCardToken
    {
        public string Cvv { get; set; }

        /// <summary>
        /// can be card number or track2 value for example 5100460000371892=21102010000024291000 or 5100460000371892
        /// </summary>
        public string CardNumber { get; set; }

        public CardExpiration CardExpiration { get; set; }

        public string CardOwnerNationalId { get; set; }
    }
}
