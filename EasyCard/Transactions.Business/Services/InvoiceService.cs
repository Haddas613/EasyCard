using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
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

        public async override Task CreateEntity(Invoice entity, IDbContextTransaction dbTransaction = null)
        {
            entity.ApplyAuditInfo(httpContextAccessor);

            await base.CreateEntity(entity, dbTransaction);
        }

        public IQueryable<Invoice> GetInvoices()
        {
            if (user.IsAdmin())
            {
                return context.Invoices.AsNoTracking();
            }
            else if (user.IsTerminal())
            {
                return context.Invoices.AsNoTracking().Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.Invoices.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public async Task<IEnumerable<Guid>> StartSending(Guid terminalID, IEnumerable<Guid> invoicesIDs, IDbContextTransaction dbTransaction)
        {
            return await context.StartSendingInvoices(terminalID, invoicesIDs, dbTransaction);
        }

        public async override Task UpdateEntity(Invoice entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.UpdateEntity(entity, dbTransaction);
        }
    }
}
