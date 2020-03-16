using Shared.Business;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class CreditCardTokenDetails : IEntityBase
    {
        public long CreditCardTokenID { get; set; }

        public string PublicKey { get; set; }

        public string Hash { get; set; }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public string CardNumber { get; set; }

        public CardExpiration CardExpiration { get; set; }

        public string CardVendor { get; set; }

        public string CardOwnerNationalID { get; set; }

        public DateTime Created { get; set; }

        public bool Active { get; set; }

        public long GetID() => CreditCardTokenID;
    }
}
