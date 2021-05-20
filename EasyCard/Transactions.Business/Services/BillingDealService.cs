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
using Transactions.Shared;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    public class BillingDealService : ServiceBase<BillingDeal, Guid>, IBillingDealService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ClaimsPrincipal user;

        public BillingDealService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public async override Task CreateEntity(BillingDeal entity, IDbContextTransaction dbTransaction = null)
        {
            entity.ApplyAuditInfo(httpContextAccessor);

            await base.CreateEntity(entity, dbTransaction);

            await AddHistory(entity.BillingDealID, string.Empty, Messages.BillingDealCreated, BillingDealOperationCodesEnum.Created);
        }

        public IQueryable<BillingDeal> GetBillingDeals()
        {
            if (user.IsAdmin())
            {
                return context.BillingDeals.AsNoTracking();
            }
            else if (user.IsTerminal())
            {
                return context.BillingDeals.AsNoTracking().Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.BillingDeals.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public IQueryable<BillingDeal> GetBillingDealsForUpdate()
        {
            if (user.IsAdmin())
            {
                return context.BillingDeals;
            }
            else if (user.IsTerminal())
            {
                return context.BillingDeals.Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.BillingDeals.Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public async override Task UpdateEntity(BillingDeal entity, IDbContextTransaction dbTransaction = null)
        {
            var exist = this.context.BillingDeals.Find(entity.GetID());

            this.context.Entry(exist).CurrentValues.SetValues(entity);

            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            entity.UpdatedDate = DateTime.UtcNow;

            //await base.UpdateEntity(entity, dbTransaction);

            if (dbTransaction != null)
            {
                await context.SaveChangesAsync();
                await AddHistory(entity.BillingDealID, changesStr, Messages.BillingDealUpdated, BillingDealOperationCodesEnum.Updated);
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await context.SaveChangesAsync();
                await AddHistory(entity.BillingDealID, changesStr, Messages.BillingDealUpdated, BillingDealOperationCodesEnum.Updated);
                await transaction.CommitAsync();
            }
        }

        public IQueryable<BillingDealHistory> GetBillingDealHistory(Guid billingDealID) =>
            context.BillingDealHistories.Where(h => h.BillingDealID == billingDealID).AsNoTracking();

        private async Task AddHistory(Guid billingDealID, string opDescription, string message, BillingDealOperationCodesEnum operationCode)
        {
            var historyRecord = new BillingDealHistory
            {
                BillingDealID = billingDealID,
                OperationCode = operationCode,
                OperationDescription = opDescription,
                OperationMessage = message
            };

            historyRecord.ApplyAuditInfo(httpContextAccessor);

            context.BillingDealHistories.Add(historyRecord);
            await context.SaveChangesAsync();
        }
    }
}
