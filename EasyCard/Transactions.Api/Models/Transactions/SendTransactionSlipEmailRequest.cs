using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class SendTransactionSlipEmailRequest
    {
        [Required]
        public Guid TransactionID { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
