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

        // TODO: as no tracking
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
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return tokens.Where(t => t.TerminalID == terminalID);
            }
            else
            {
                var response = tokens.Where(t => t.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID()?.Cast<Guid?>();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.TerminalID));
                }

                return response;
            }
        }

        public IQueryable<CreditCardTokenDetails> GetTokensShared(Guid baseTerminalID)
        {
            var tokens = context.CreditCardTokenDetails.AsQueryable();

            tokens = tokens.Where(t => t.Active);

            if (user.IsAdmin())
            {
                throw new ApplicationException("This method should not be used by Admin");
            }
            else
            {
                // NOTE: assign user to terminal will not work in this case
                var response = tokens.Where(t => t.MerchantID == user.GetMerchantID())
                    .Where(d => d.InitialTransactionID == null || d.TerminalID == baseTerminalID);

                return response;
            }
        }

        public IQueryable<CreditCardTokenDetails> GetTokensSharedAdmin(Guid merchantID, Guid baseTerminalID)
        {
            var tokens = context.CreditCardTokenDetails.AsQueryable();

            tokens = tokens.Where(t => t.Active);

            if (!user.IsAdmin())
            {
                throw new ApplicationException("This method should only be used by Admin");
            }
            else
            {
                // NOTE: assign user to terminal will not work in this case
                var response = tokens.Where(t => t.MerchantID == merchantID)
                    .Where(d => d.InitialTransactionID == null || d.TerminalID == baseTerminalID);

                return response;
            }
        }

        public async override Task UpdateEntity(CreditCardTokenDetails entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.UpdateEntity(entity, dbTransaction);
        }

        public async Task<bool> ConsumerCCTokenExistsAsync(Guid consumerID)
        {
            return await context.CreditCardTokenDetails.AnyAsync(t => t.ConsumerID == consumerID);
        }
    }
}
