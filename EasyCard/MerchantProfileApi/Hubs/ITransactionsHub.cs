using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Hubs
{
    public interface ITransactionsHub
    {
        Task TransactionStatusChanged(object payload);
    }
}
