using Merchants.Business.Data;
using Merchants.Business.Entities.Merchant;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class ImpersonationService : IImpersonationService
    {
        private readonly MerchantsContext dataContext;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ILogger logger;

        public ImpersonationService(MerchantsContext dataContext, IHttpContextAccessorWrapper httpContextAccessor, ILogger<ImpersonationService> logger)
        {
            this.dataContext = dataContext;
            this.httpContextAccessor = httpContextAccessor;

            this.user = this.httpContextAccessor.GetUser();

            this.logger = logger;
        }

        public async Task<OperationResponse> Impersonate(Guid userId, Guid merchantID)
        {
            var impersonation = await this.dataContext.Impersonations.FirstOrDefaultAsync(d => d.UserId == userId);

            if (impersonation == null)
            {
                impersonation = new Impersonation { UserId = userId };
                this.dataContext.Impersonations.Add(impersonation);
            }

            impersonation.MerchantID = merchantID;

            return await SaveChagesSafe();
        }

        public async Task<OperationResponse> LoginAsMerchant(Guid merchantID)
        {
            var userId = this.user.GetDoneByID();
            var impersonation = await this.dataContext.Impersonations.FirstOrDefaultAsync(d => d.UserId == userId);

            if (impersonation == null)
            {
                impersonation = new Impersonation { UserId = userId.Value };
                this.dataContext.Impersonations.Add(impersonation);
            }

            impersonation.MerchantID = merchantID;

            return await SaveChagesSafe();
        }

        public async Task SetImpersonationClaims(ClaimsPrincipal principal)
        {
            if (principal.IsInteractiveAdmin())
            {
                var userId = principal.GetDoneByID();
                var impersonation = await GetImpersonatedMerchantID(userId.Value);

                if (impersonation != null)
                {
                    var impersonationIdentity = new ClaimsIdentity(new[]
                    {
                        new Claim(Claims.MerchantIDClaim, impersonation.ToString()),
                        new Claim(ClaimTypes.Role, Roles.Merchant)
                    });
                    principal?.AddIdentity(impersonationIdentity);
                }
            }
            else if (principal.IsMerchant())
            {
                var userId = principal.GetDoneByID();
                var impersonation = await GetImpersonatedData(userId.Value);

                if (impersonation != null)
                {
                    var midentity = (ClaimsIdentity)principal.Identity;
                    foreach (var midclaim in midentity.FindAll(Claims.MerchantIDClaim).ToList())
                    {
                        midentity.TryRemoveClaim(midclaim);
                    }

                    midentity.AddClaim(new Claim(Claims.MerchantIDClaim, impersonation.MerchantID.ToString()));

                    foreach (var tidclaim in midentity.FindAll(Claims.TerminalIDClaim).ToList())
                    {
                        midentity.TryRemoveClaim(tidclaim);
                    }

                    if (impersonation.Terminals?.Count() > 0)
                    {
                        foreach (var tid in impersonation.Terminals)
                        {
                            midentity.AddClaim(new Claim(Claims.TerminalIDClaim, tid.ToString()));
                        }
                    }
                }
            }
        }

        private async Task<OperationResponse> SaveChagesSafe()
        {
            var operationResult = new OperationResponse { Status = StatusEnum.Success };

            using (var transaction = this.dataContext.Database.BeginTransaction())
            {
                try
                {
                    await dataContext.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var correlationId = httpContextAccessor.GetCorrelationId();

                    logger.LogError(ex, $"Exception {correlationId}: {ex.Message}");

                    operationResult = new OperationResponse(ex.Message, StatusEnum.Error, correlationId: correlationId);
                }
            }

            return operationResult;
        }

        private async Task<ImpersonationData> GetImpersonatedData(Guid userId)
        {
            var impersonation = await this.dataContext.Impersonations
                .Join(this.dataContext.UserTerminalMappings, d => d.UserId, d => d.UserID, (i, m) => new { i.UserId, i.MerchantID, m.Terminals })
                .Where(d => d.UserId == userId)
                .Select(d => new ImpersonationData(d.MerchantID, d.Terminals))
                .FirstOrDefaultAsync();

            return impersonation;
        }

        private async Task<Guid?> GetImpersonatedMerchantID(Guid userId)
        {
            var impersonation = await this.dataContext.Impersonations.FirstOrDefaultAsync(d => d.UserId == userId);

            return impersonation?.MerchantID;
        }

        private class ImpersonationData
        {
            public ImpersonationData(Guid? merchantID, IEnumerable<Guid> terminals)
            {
                MerchantID = merchantID;
                Terminals = terminals;
            }

            public Guid? MerchantID { get; set; }

            public IEnumerable<Guid> Terminals { get; set; }
        }
    }
}
