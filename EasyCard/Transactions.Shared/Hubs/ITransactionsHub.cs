using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Transactions.Shared.Models;

namespace Transactions.Shared.Hubs
{
    public interface ITransactionsHub
    {
        Task TransactionStatusChanged(TransactionStatusChangedHubModel payload);
    }
}
