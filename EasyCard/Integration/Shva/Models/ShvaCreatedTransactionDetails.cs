using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    public class ShvaCreatedTransactionDetails
    {
        /// <summary>
        ///  Shva Shovar Number
        /// </summary>
        public string ShvaShovarNumber { get; set; }

        /// <summary>
        /// Shva Deal ID
        /// </summary>
        public string ShvaDealID { get; set; }

        // TODO: how to get this ?
        public string AuthNum { get; set; }

        // TODO: how to get this ?
        public string AuthSolekNum { get; set; }

        /// <summary>
        /// TODO: when to fill it ?
        /// </summary>
        public DateTime? TransactionDate { get; set; }
    }
}
