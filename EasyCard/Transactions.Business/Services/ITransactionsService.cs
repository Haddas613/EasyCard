using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface ITransactionsService : IServiceBase<PaymentTransaction>
    {
        IQueryable<PaymentTransaction> GetTransactions();
    }
}
