using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public class CreditCardDetailsBase
    {
        public string CardNumber { get; set; }

        public string CardBin { get; set; }

        public CardExpiration CardExpiration { get; set; }

        public string CardVendor { get; set; }

        public string CardOwnerNationalID { get; set; }
    }
}
