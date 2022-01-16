using Merchants.Business.Data;
using Merchants.Business.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Merchants.Business.Services
{
    public class ConsumersService : ServiceBase<Consumer, Guid>, IConsumersService
    {
        private readonly MerchantsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public ConsumersService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<Consumer> GetConsumers()
        {
            if (user.IsAdmin())
            {
                return context.Consumers.AsNoTracking();
            }
            //else if (user.IsTerminal())
            //{
            //    var terminalID = user.GetTerminalID()?.FirstOrDefault();
            //    return context.Consumers.AsNoTracking().Where(t => t.TerminalID == terminalID);
            //}
            else
            {
                var terminals = user.GetTerminalID();
                var consumers = context.Consumers.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
                //if (terminals?.Count() > 0)
                //{
                //    consumers = consumers.Where(d => terminals.Contains(d.TerminalID));
                //}

                return consumers;
            }
        }
    }
}
