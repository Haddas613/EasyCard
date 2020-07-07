using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    /// <summary>
    /// Transmission cancelation request
    /// </summary>
    public class CancelTransmissionRequest
    {
        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// ID of transactions which needs to be canceled
        /// </summary>
        public Guid PaymentTransactionID { get; set; }
    }
}
