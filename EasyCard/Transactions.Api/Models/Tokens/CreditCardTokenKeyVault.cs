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
    public class CreditCardTokenKeyVault
    {
        public string Cvv { get; set; }

        public string CardNumber { get; set; }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public CardExpiration CardExpiration { get; set; }
    }
}
