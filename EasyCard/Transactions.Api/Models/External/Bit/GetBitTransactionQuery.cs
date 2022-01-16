using System;
using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.Models.External.Bit
{
    public class GetBitTransactionQuery
    {
        /// <summary>
        /// ECNG transaction id
        /// </summary>
        public Guid PaymentTransactionID { get; set; }

        /// <summary>
        /// Bit transaction id (BitPaymentInitiationId)
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string PaymentInitiationId { get; set; }

        /// <summary>
        /// Bit transaction serial id (BitTransactionSerialId)
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string TransactionSerialId { get; set; }
    }
}
