using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.AutoHistory;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;
using Transactions.Shared;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    public class PaymentRequestsService : ServiceBase<PaymentRequest, Guid>, IPaymentRequestsService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public PaymentRequestsService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async override Task CreateEntity(PaymentRequest entity, IDbContextTransaction dbTransaction = null)
        {
            entity.ApplyAuditInfo(httpContextAccessor);

            await base.CreateEntity(entity, dbTransaction);
        }

        public IQueryable<PaymentRequest> GetPaymentRequests() => context.PaymentRequests;

        public IQueryable<PaymentRequestHistory> GetPaymentRequestHistory(Guid paymentRequestID) => context.PaymentRequestHistories.Where(d => d.PaymentRequestID == paymentRequestID);

        public async override Task UpdateEntity(PaymentRequest entity, IDbContextTransaction dbTransaction = null)
            => await UpdateEntity(entity, Messages.TransactionUpdated, PaymentRequestOperationCodesEnum.PaymentRequestUpdated, dbTransaction: dbTransaction);

        public async Task UpdateEntityWithStatus(PaymentRequest entity, PaymentRequestStatusEnum? status = null, string message = null, Guid? paymentTransactionID = null, IDbContextTransaction dbTransaction = null)
        {
            entity.Status = status ?? entity.Status;
            entity.PaymentTransactionID = paymentTransactionID ?? entity.PaymentTransactionID;

            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            entity.UpdatedDate = DateTime.UtcNow;

            PaymentRequestOperationCodesEnum operationCode = PaymentRequestOperationCodesEnum.PaymentRequestUpdated;
            var historyMessage = Messages.PaymentRequestUpdated;

            if (status != null)
            {
                var parsedRes = Enum.TryParse(status?.ToString(), true, out operationCode);
            }

            historyMessage = message ?? (Messages.ResourceManager.GetString(operationCode.ToString()) ?? historyMessage);

            if (dbTransaction != null)
            {
                await base.UpdateEntity(entity, dbTransaction);
                await AddHistory(entity.PaymentRequestID, changesStr, historyMessage, operationCode);
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.UpdateEntity(entity, transaction);
                await AddHistory(entity.PaymentRequestID, changesStr, historyMessage, operationCode);
                await transaction.CommitAsync();
            }
        }

        private async Task UpdateEntity(PaymentRequest entity, string historyMessage, PaymentRequestOperationCodesEnum operationCode, IDbContextTransaction dbTransaction = null)
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

            if (dbTransaction != null)
            {
                await base.UpdateEntity(entity, dbTransaction);
                await AddHistory(entity.PaymentRequestID, changesStr, historyMessage, operationCode);
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.UpdateEntity(entity, transaction);
                await AddHistory(entity.PaymentRequestID, changesStr, historyMessage, operationCode);
                await transaction.CommitAsync();
            }
        }

        private async Task AddHistory(Guid paymentRequestID, string opDescription, string message, PaymentRequestOperationCodesEnum operationCode)
        {
            var historyRecord = new PaymentRequestHistory
            {
                PaymentRequestID = paymentRequestID,
                OperationCode = operationCode,
                OperationDescription = opDescription,
                OperationMessage = message,
            };

            historyRecord.ApplyAuditInfo(httpContextAccessor);

            context.PaymentRequestHistories.Add(historyRecord);
            await context.SaveChangesAsync();
        }
    }
}
