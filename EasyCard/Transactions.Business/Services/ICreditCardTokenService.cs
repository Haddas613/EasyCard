using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface ICreditCardTokenService : IServiceBase<CreditCardTokenDetails, Guid>
    {
        IQueryable<CreditCardTokenDetails> GetTokens(bool showIncactive = false);

        IQueryable<CreditCardTokenDetails> GetTokensShared(Guid baseTerminalID);

        IQueryable<CreditCardTokenDetails> GetTokensSharedAdmin(Guid merchantID, Guid baseTerminalID);
    }
}
