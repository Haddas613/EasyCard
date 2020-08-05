using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface IInvoiceService : IServiceBase<Invoice, Guid>
    {
        IQueryable<Invoice> GetInvoices();
    }
}
