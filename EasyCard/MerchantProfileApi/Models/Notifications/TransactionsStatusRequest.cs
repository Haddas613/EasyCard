using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace MerchantProfileApi.Models.Notifications
{
    public class TransactionsStatusRequest
    {
        public Guid PaymentTransactionID { get; set; }

        public TransactionStatusEnum Status { get; set; }

        public Guid UserID { get; set; }
    }
}
