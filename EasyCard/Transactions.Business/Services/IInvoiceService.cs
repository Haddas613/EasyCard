using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface IInvoiceService : IServiceBase<Invoice, Guid>
    {
        IQueryable<Invoice> GetInvoices();

        Task<IEnumerable<Guid>> StartSending(Guid terminalID, IEnumerable<Guid> invoicesIDs, IDbContextTransaction dbTransaction);

        IQueryable<InvoiceHistory> GetInvoiceHistory(Guid invoiceID);

        Task<Invoice> GetInvoice(Guid invoiceID);
    }
}
