﻿using Microsoft.EntityFrameworkCore;
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
    public class ThreeDSChallengeService : ServiceBase<ThreeDSChallenge, Guid>, IThreeDSChallengeService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ClaimsPrincipal user;

        public ThreeDSChallengeService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public async Task<ThreeDSChallenge> GetThreeDSChallenge(string threeDSServerTransID)
        {
            return await context.ThreeDSChallenges.FirstOrDefaultAsync(d => d.ThreeDSServerTransID == threeDSServerTransID);
        }
    }
}
