using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.AutoHistory;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;
using Transactions.Shared.Messages;

namespace Transactions.Business.Services
{
    public class TransactionsService : ServiceBase<PaymentTransaction>, ITransactionsService
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
            await base.CreateEntity(entity, dbTransaction);

            await AddHistory(entity.PaymentTransactionID, string.Empty, DbLayerMessages.TransactionCreated, TransactionOperationCodesEnum.TransactionCreated, null);
        }


        public async override Task UpdateEntity(PaymentTransaction entity, IDbContextTransaction dbTransaction = null) 
            => await UpdateEntityWithStatus(entity, DbLayerMessages.TransactionUpdated, TransactionOperationCodesEnum.TransactionUpdated, dbTransaction);

        public async Task UpdateEntityWithStatus(PaymentTransaction entity, string historyMessage, TransactionOperationCodesEnum operationCode, 
            IDbContextTransaction dbTransaction = null, string integrationMessageId = null)
        {
            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            await base.UpdateEntity(entity, dbTransaction);
            await AddHistory(entity.PaymentTransactionID, changesStr, historyMessage, operationCode, integrationMessageId);
        }

        private async Task AddHistory(long transactionID, string opDescription, string message, TransactionOperationCodesEnum operationCode, string integrationMessageId = null)
        {
            var historyRecord = new TransactionHistory
            {
                OperationDoneBy = user.GetDoneBy(),
                OperationDoneByID = user.GetDoneByID(),
                PaymentTransactionID = transactionID,
                OperationCode = operationCode.ToString(),
                OperationDate = DateTime.UtcNow,
                OperationDescription = opDescription,
                CorrelationId = httpContextAccessor.GetCorrelationId(),
                OperationMessage = message,
                IntegrationMessageId = integrationMessageId
            };

            context.TransactionHistories.Add(historyRecord);
            await context.SaveChangesAsync();
        }
    }
}
