﻿using Merchants.Business.Data;
using Merchants.Business.Entities.Billing;
using Shared.Business;
using Shared.Business.Security;
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

        public IQueryable<Consumer> GetConsumers() => context.Consumers;
    }
}
