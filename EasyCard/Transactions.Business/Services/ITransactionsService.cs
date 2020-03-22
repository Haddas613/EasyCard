using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    public interface ITransactionsService : IServiceBase<PaymentTransaction>
    {
        IQueryable<PaymentTransaction> GetTransactions();

        Task UpdateEntityWithStatus(PaymentTransaction entity, string historyMessage, TransactionOperationCodesEnum operation, 
            IDbContextTransaction dbTransaction = null, string integrationMessageId = null);
    }
}
