using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class ShvaInitialTransactionDetails
    {
        /// <summary>
        /// Shva Deal ID
        /// </summary>
        public string ShvaDealID { get; set; }

        // TODO: encrypt
        public string AuthNum { get; set; }

        public string AuthSolekNum { get; set; }

        /// <summary>
        /// Deal Date
        /// </summary>
        public DateTime? ShvaTransactionDate { get; set; }
    }
}
