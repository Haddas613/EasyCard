using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    /// <summary>
    /// To be used instead of <see cref="ITransactionsService"/> in case when authorization context is not available.
    /// </summary>
    public interface ITransactionsDirectAccessService
    {
        IQueryable<PaymentTransaction> GetTransactions();
    }
}
