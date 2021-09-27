using Merchants.Business.Entities.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Api.Services.Notification
{
    public interface INotificationService
    {
        Task NotifyTransactionStatus(PaymentTransaction transaction, Terminal terminal);
    }
}
