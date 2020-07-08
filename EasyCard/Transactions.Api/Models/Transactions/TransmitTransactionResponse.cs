using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class TransmitTransactionResponse
    {
        public Guid? PaymentTransactionID { get; set; }

        public TransmissionStatusEnum TransmissionStatus { get; set; }
    }
}
