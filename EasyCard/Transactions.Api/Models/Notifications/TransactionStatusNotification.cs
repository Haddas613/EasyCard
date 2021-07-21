using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Notifications
{
    public class TransactionStatusNotification
    {
        public Guid PaymentTransactionID { get; set; }

        public TransactionStatusEnum Status { get; set; }

        public Guid? UserID { get; set; }
    }
}
