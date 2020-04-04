using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class CreditCardDetails : CreditCardDetailsBase
    {
        public string CardBin { get; set; }

        public string CardLastFourDigits { get; set; }
    }
}
