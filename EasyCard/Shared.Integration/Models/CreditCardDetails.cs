using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class CreditCardDetails
    {
        public CardExpiration CardExpiration { get; set; }

        public string CardBin { get; set; }

        public string CardLastFourDigits { get; set; }

        public string CardVendor { get; set; }

        public string CardOwnerNationalId { get; set; }

        public string CardOwnerName { get; set; }

        public bool? IsTourist { get; set; }
    }
}
