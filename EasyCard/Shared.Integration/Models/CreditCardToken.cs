using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class CreditCardToken
    {
        public string Cvv { get; set; }

        public string CardNumber { get; set; }

        public long TerminalID { get; set; }

        public string UserID { get; set; }

        public CardExpiration CardExpiration { get; set; }
    }
}
