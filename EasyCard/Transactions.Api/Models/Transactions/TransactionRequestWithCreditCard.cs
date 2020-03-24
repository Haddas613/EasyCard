using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class TransactionRequestWithCreditCard : TransactionRequest
    {
        [Required]
        public CreditCardSecureDetails CreditCardSecureDetails { get; set; }
    }
}
