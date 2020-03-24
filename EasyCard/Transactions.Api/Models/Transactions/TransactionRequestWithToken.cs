using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class TransactionRequestWithToken : TransactionRequest
    {
        [Required]
        public string CardToken { get; set; }
    }
}
