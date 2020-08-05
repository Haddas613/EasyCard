using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Invoicing;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class InvoiceFilteringExtensions
    {
        public static IQueryable<Invoice> Filter(this IQueryable<Invoice> src, InvoicesFilter filter)
        {
            return src;
        }
    }
}
