using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Api.Services
{
    public interface IMetricsService
    {
        void TrackTransactionEvent(PaymentTransaction paymentTransaction, TransactionOperationCodesEnum eventName);
    }
}
