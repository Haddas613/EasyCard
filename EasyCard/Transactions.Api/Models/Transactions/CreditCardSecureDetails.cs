using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class CreditCardSecureDetails
    {
        [Required]
        public string CardOwnerName { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 3)]
        public string Cvv { get; set; }

        [Required]
        public string CardNumber { get; set; }

        [Required]
        public CardExpiration CardExpiration { get; set; }

        public string CardOwnerNationalID { get; set; }
    }
}
