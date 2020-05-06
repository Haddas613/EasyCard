using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    /// <summary>
    /// Transmission request
    /// </summary>
    public class TransmitTransactionsRequest
    {
        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// IDs of transactions which needs to be transmitted
        /// </summary>
        /// <remarks>
        /// If omitted, all non-transmitted transactions will be transmitted
        /// </remarks>
        public IEnumerable<Guid> PaymentTransactionIDs { get; set; }
    }
}
