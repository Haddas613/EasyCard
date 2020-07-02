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
    public interface ITransactionsService : IServiceBase<PaymentTransaction, Guid>
    {
        IQueryable<PaymentTransaction> GetTransactions();

        Task UpdateEntityWithStatus(PaymentTransaction entity, TransactionStatusEnum transactionStatus, TransactionFinalizationStatusEnum? finalizationStatus = null, IDbContextTransaction dbTransaction = null);
    }
}
