using Shared.Business;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface INayaxTransactionsParametersService : IServiceBase<NayaxTransactionsParameters, Guid>
    {
        Task<NayaxTransactionsParameters> GetNayaxTransactionsParameter(string vuid);

    }
}
