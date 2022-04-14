using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;
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
using Z.EntityFramework.Plus;
using SharedApi = Shared.Api;

namespace Transactions.Business.Services
{
    public class BillingDealService : ServiceBase<BillingDeal, Guid>, IBillingDealService, IFutureBillingsService
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
                return context.BillingDeals;
            }
            else if (user.IsTerminal())
            {
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return context.BillingDeals.Where(t => t.TerminalID == terminalID);
            }
            else
            {
                var response = context.BillingDeals.Where(t => t.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.TerminalID));
                }

                return response;
            }
        }

        public async Task<BillingDeal> GetBillingDeal(Guid billingDeal)
        {
            if (user.IsAdmin())
            {
                return await context.BillingDeals.FirstOrDefaultAsync(d => d.BillingDealID == billingDeal);
            }
            else if (user.IsTerminal())
            {
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return await context.BillingDeals.Where(t => t.TerminalID == terminalID).FirstOrDefaultAsync(d => d.BillingDealID == billingDeal);
            }
            else
            {
                var response = context.BillingDeals.Where(t => t.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.TerminalID));
                }

                return await response.FirstOrDefaultAsync(d => d.BillingDealID == billingDeal);
            }
        }

        public async override Task UpdateEntity(BillingDeal entity, IDbContextTransaction dbTransaction = null)
            => await UpdateEntityWithHistory(entity, Messages.BillingDealUpdated, BillingDealOperationCodesEnum.Updated, dbTransaction);

        public IQueryable<BillingDealHistory> GetBillingDealHistory(Guid billingDealID) =>
            context.BillingDealHistories.Where(h => h.BillingDealID == billingDealID).AsNoTracking();

        public async Task UpdateEntityWithHistory(BillingDeal entity, string message, BillingDealOperationCodesEnum operationCode, IDbContextTransaction dbTransaction = null)
        {
            var exist = this.context.BillingDeals.Find(entity.GetID());

            //this.context.Entry(exist).CurrentValues.SetValues(entity);

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
                await context.SaveChangesAsync();
                await AddHistory(entity.BillingDealID, changesStr, message, operationCode);
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await context.SaveChangesAsync();
                await AddHistory(entity.BillingDealID, changesStr, message, operationCode);
                await transaction.CommitAsync();
            }
        }

        public IQueryable<FutureBilling> GetFutureBillings()
        {
            if (user.IsAdmin())
            {
                return context.FutureBillings.AsNoTracking();
            }
            else if (user.IsTerminal())
            {
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return context.FutureBillings.AsNoTracking().Where(t => t.TerminalID == terminalID);
            }
            else
            {
                var response = context.FutureBillings.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.TerminalID));
                }

                return response;
            }
        }

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
