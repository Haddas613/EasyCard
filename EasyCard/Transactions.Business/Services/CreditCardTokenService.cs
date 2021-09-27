using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class CreditCardTokenService : ServiceBase<CreditCardTokenDetails, Guid>, ICreditCardTokenService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ClaimsPrincipal user;

        public CreditCardTokenService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public async override Task CreateEntity(CreditCardTokenDetails entity, IDbContextTransaction dbTransaction = null)
        {
            entity.ApplyAuditInfo(httpContextAccessor);

            await base.CreateEntity(entity, dbTransaction);
        }

        public IQueryable<CreditCardTokenDetails> GetTokens(bool showIncactive = false)
        {
            var tokens = context.CreditCardTokenDetails.AsQueryable();

            if (!showIncactive)
            {
                tokens = tokens.Where(t => t.Active);
            }

            if (user.IsAdmin())
            {
                return tokens;
            }
            else if (user.IsTerminal())
            {
                return tokens.Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return tokens.Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public async override Task UpdateEntity(CreditCardTokenDetails entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.UpdateEntity(entity, dbTransaction);
        }
    }
}
