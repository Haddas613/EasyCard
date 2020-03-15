using Shared.Business;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    /// <summary>
    /// Entity to store in SQL database to control tokens operations
    /// </summary>
    public class CreditCardTokenDetails : CreditCardDetailsBase, IEntityBase
    {
        public long CreditCardTokenID { get; set; }

        /// <summary>
        /// Refrerence to be used by merchant
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Provate credit card token reference
        /// </summary>
        public string Hash { get; set; }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public DateTime Created { get; set; }

        public bool Active { get; set; }

        public long GetID() => CreditCardTokenID;
    }
}
