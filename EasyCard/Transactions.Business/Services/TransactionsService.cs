using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.AutoHistory;
using Shared.Business.Exceptions;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;
using Transactions.Shared;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    public class TransactionsService : ServiceBase<PaymentTransaction, Guid>, ITransactionsService
    {
        private readonly TransactionsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public TransactionsService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor) : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<CreditCardTokenDetails> GetTokens() => context.CreditCardTokenDetails;

        public IQueryable<PaymentTransaction> GetTransactions() => context.PaymentTransactions;

        public async override Task CreateEntity(PaymentTransaction entity, IDbContextTransaction dbTransaction = null)
        {
            if (user.IsTerminal() && entity.TerminalID != user.GetTerminalID() || user.IsMerchant() && entity.MerchantID != user.GetMerchantID())
            {
                throw new SecurityException(Messages.PleaseCheckValues);
            }

            entity.UpdatedDate = DateTime.UtcNow;

            if (dbTransaction != null)
            {
                await base.CreateEntity(entity, dbTransaction);
                await AddHistory(entity.PaymentTransactionID, string.Empty, Messages.TransactionCreated, TransactionOperationCodesEnum.TransactionCreated);
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.CreateEntity(entity, transaction);
                await AddHistory(entity.PaymentTransactionID, string.Empty, Messages.TransactionCreated, TransactionOperationCodesEnum.TransactionCreated);
                await transaction.CommitAsync();
            }
        }

        public async override Task UpdateEntity(PaymentTransaction entity, IDbContextTransaction dbTransaction = null) 
            => await UpdateEntity(entity, Messages.TransactionUpdated, TransactionOperationCodesEnum.TransactionUpdated, dbTransaction: dbTransaction);

        public async Task UpdateEntityWithStatus(PaymentTransaction entity, TransactionStatusEnum transactionStatus, TransactionFinalizationStatusEnum? finalizationStatus = null, IDbContextTransaction dbTransaction = null)
        {
            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            entity.UpdatedDate = DateTime.UtcNow;

            TransactionOperationCodesEnum operationCode = TransactionOperationCodesEnum.TransactionUpdated;
            var historyMessage = Messages.TransactionUpdated;

            entity.Status = transactionStatus;
            Enum.TryParse<TransactionOperationCodesEnum>(transactionStatus.ToString(), true, out operationCode);
            historyMessage = Messages.ResourceManager.GetString(operationCode.ToString()) ?? historyMessage;

            if (dbTransaction != null)
            {
                await base.UpdateEntity(entity, dbTransaction);
                await AddHistory(entity.PaymentTransactionID, changesStr, historyMessage, operationCode);
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.UpdateEntity(entity, transaction);
                await AddHistory(entity.PaymentTransactionID, changesStr, historyMessage, operationCode);
                await transaction.CommitAsync();
            }
        }

        private async Task UpdateEntity(PaymentTransaction entity, string historyMessage, TransactionOperationCodesEnum operationCode, IDbContextTransaction dbTransaction = null)
        {
            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            entity.UpdatedDate = DateTime.UtcNow;

            if(dbTransaction != null)
            {
                await base.UpdateEntity(entity, dbTransaction);
                await AddHistory(entity.PaymentTransactionID, changesStr, historyMessage, operationCode);
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.UpdateEntity(entity, transaction);
                await AddHistory(entity.PaymentTransactionID, changesStr, historyMessage, operationCode);
                await transaction.CommitAsync();
            }
        }

        private async Task AddHistory(Guid transactionID, string opDescription, string message, TransactionOperationCodesEnum operationCode)
        {
            var historyRecord = new TransactionHistory
            {
                PaymentTransactionID = transactionID,
                OperationCode = operationCode,
                OperationDescription = opDescription,
                OperationMessage = message,
            };

            historyRecord.ApplyAuditInfo(httpContextAccessor);

            context.TransactionHistories.Add(historyRecord);
            await context.SaveChangesAsync();
        }
    }
}
