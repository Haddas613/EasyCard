using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Hubs
{
    public interface ITransactionsHub
    {
        //public Dictionary<Guid, string> Connections { get; }

        Task TransactionStatusChanged(object payload);
    }
}
