using Microsoft.EntityFrameworkCore.Storage;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    /// <summary>
    /// To be used instead of <see cref="ITransactionsService"/> in case when authorization context is not available.
    /// </summary>
    public interface ITransactionsDirectAccessService
    {
        IQueryable<PaymentTransaction> GetTransactions();

        Task UpdateEntityWithStatus(PaymentTransaction entity, TransactionStatusEnum? transactionStatus = null, TransactionFinalizationStatusEnum? finalizationStatus = null, RejectionReasonEnum? rejectionReason = null, string rejectionMessage = null, IDbContextTransaction dbTransaction = null, TransactionOperationCodesEnum? transactionOperationCode = null);

    }
}
