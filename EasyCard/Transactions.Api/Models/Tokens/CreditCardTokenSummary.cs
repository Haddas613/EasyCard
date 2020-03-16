using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Tokens
{
    public class CreditCardTokenSummary
    {
        public long CreditCardTokenID { get; set; }

        public string PublicKey { get; set; }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public string CardNumber { get; set; }

        public CardExpiration CardExpiration { get; set; }

        public string CardVendor { get; set; }

        public string CardOwnerNationalID { get; set; }

        public DateTime Created { get; set; }
    }
}
