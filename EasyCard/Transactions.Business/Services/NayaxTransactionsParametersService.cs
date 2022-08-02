using Microsoft.EntityFrameworkCore;
using Shared.Business;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class NayaxTransactionsParametersService : ServiceBase<NayaxTransactionsParameters, Guid>, INayaxTransactionsParametersService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ClaimsPrincipal user;

        public NayaxTransactionsParametersService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public Task<NayaxTransactionsParameters> GetNayaxTransactionsParameter(string vuid)
        {
            return context.NayaxTransactionsParameters.AsNoTracking().FirstOrDefaultAsync(p => p.PinPadTransactionID == vuid);
        }
    }
}
