using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class ChargebackRequest
    {
        [Required]
        public Guid ExistingPaymentTransactionID { get; set; }

        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        [Required]
        public decimal RefundAmount { get; set; }
    }
}
