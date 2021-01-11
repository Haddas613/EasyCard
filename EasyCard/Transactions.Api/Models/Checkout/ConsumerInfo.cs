using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Checkout
{
    public class ConsumerInfo
    {
        public Guid ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        public string ConsumerName { get; set; }

        public string ConsumerPhone { get; set; }

        public string ConsumerNationalID { get; set; }

        public IEnumerable<TokenInfo> Tokens { get; set; }
    }

    public class TokenInfo
    {
        public Guid CreditCardTokenID { get; set; }

        public string CardNumber { get; set; }

        public string CardExpiration { get; set; }

        public string CardVendor { get; set; }
    }
}
