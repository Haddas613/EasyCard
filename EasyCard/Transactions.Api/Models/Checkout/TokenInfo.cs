using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Checkout
{
    public class TokenInfo
    {
        public Guid CreditCardTokenID { get; set; }

        public string CardNumber { get; set; }

        public string CardExpiration { get; set; }

        public string CardVendor { get; set; }

        public DateTime? Created { get; set; }
    }
}
