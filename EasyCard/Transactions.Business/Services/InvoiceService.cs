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
    public class InvoiceService : ServiceBase<Invoice, Guid>, IInvoiceService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ClaimsPrincipal user;

        public InvoiceService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<InvoiceHistory> GetInvoiceHistory(Guid invoiceID)
        {
            return GetInvoiceHistories().Where(i => i.InvoiceID == invoiceID);
        }

        public IQueryable<InvoiceHistory> GetInvoiceHistories()
        {
            if (user.IsAdmin())
            {
                return context.InvoiceHistories.AsNoTracking();
            }
            else if (user.IsTerminal())
            {
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return context.InvoiceHistories.AsNoTracking().Where(t => t.Invoice.TerminalID == terminalID);
            }
            else
            {
                var response = context.InvoiceHistories.AsNoTracking().Where(t => t.Invoice.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.Invoice.TerminalID));
                }

                return response;
            }
        }

        public IQueryable<Invoice> GetInvoices()
        {
            if (user.IsAdmin())
            {
                return context.Invoices.AsNoTracking();
            }
            else if (user.IsTerminal())
            {
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return context.Invoices.AsNoTracking().Where(t => t.TerminalID == terminalID);
            }
            else
            {
                var response = context.Invoices.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.TerminalID));
                }

                return response;
            }
        }

        public async Task<IEnumerable<Guid>> StartSending(Guid terminalID, IEnumerable<Guid> invoicesIDs, IDbContextTransaction dbTransaction)
        {
            return await context.StartSendingInvoices(terminalID, invoicesIDs, dbTransaction);
        }

        public async override Task CreateEntity(Invoice entity, IDbContextTransaction dbTransaction = null)
        {
            entity.ApplyAuditInfo(httpContextAccessor);
            await base.CreateEntity(entity, dbTransaction);

            await AddHistory(entity.InvoiceID, string.Empty, Messages.InvoiceCreated, InvoiceOperationCodesEnum.InvoiceCreated);
        }

        public async override Task UpdateEntity(Invoice entity, IDbContextTransaction dbTransaction = null)
        {
            entity.UpdatedDate = DateTime.UtcNow;
            await base.UpdateEntity(entity, dbTransaction);

            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            await AddHistory(entity.InvoiceID, string.Empty, Messages.InvoiceUpdated, InvoiceOperationCodesEnum.InvoiceUpdated);
        }

        private async Task AddHistory(Guid invoiceID, string opDescription, string message, InvoiceOperationCodesEnum operationCode)
        {
            var historyRecord = new InvoiceHistory
            {
                InvoiceID = invoiceID,
                OperationCode = operationCode,
                OperationDescription = opDescription,
                OperationMessage = message,
            };

            historyRecord.ApplyAuditInfo(httpContextAccessor);

            context.InvoiceHistories.Add(historyRecord);
            await context.SaveChangesAsync();
        }
    }
}
