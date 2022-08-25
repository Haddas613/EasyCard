using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    public interface ITransactionsService : IServiceBase<PaymentTransaction, Guid>
    {
        IQueryable<PaymentTransaction> GetTransactions();

        IQueryable<PaymentTransaction> GetTransactionsForUpdate();

        Task<PaymentTransaction> GetTransaction(Expression<Func<PaymentTransaction, bool>> predicate);

        IQueryable<TransactionHistory> GetTransactionHistory(Guid transactionID);

        Task UpdateEntityWithStatus(PaymentTransaction entity, TransactionStatusEnum? transactionStatus = null, TransactionFinalizationStatusEnum? finalizationStatus = null, RejectionReasonEnum? rejectionReason = null, string rejectionMessage = null, IDbContextTransaction dbTransaction = null, TransactionOperationCodesEnum? transactionOperationCode = null);

        Task UpdateEntity(PaymentTransaction entity, string historyMessage, TransactionOperationCodesEnum operationCode, IDbContextTransaction dbTransaction = null);

        Task<IEnumerable<TransmissionInfo>> StartTransmission(Guid terminalID, IEnumerable<Guid> transactionIDs, IDbContextTransaction dbTransaction = null);

        Task<IEnumerable<TransactionSummaryDb>> GetGroupedTransactionSummaries(Guid? terminalID, IDbContextTransaction dbTransaction);

        Task Refresh(PaymentTransaction transaction);

        PaymentTransaction Clone(PaymentTransaction transaction);

        Task<bool> CheckDuplicateTransaction(Guid? terminalID, Guid? paymentIntentID, Guid? paymentRequestID, DateTime? threshold, decimal amount, string cardNumber, IDbContextTransaction dbContextTransaction, JDealTypeEnum jDealType);
    }
}
