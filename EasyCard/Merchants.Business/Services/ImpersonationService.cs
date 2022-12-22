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

        public async Task<Guid?> GetImpersonatedMerchantID(Guid userId)
        {
            var impersonation = await this.dataContext.Impersonations.FirstOrDefaultAsync(d => d.UserId == userId);

            return impersonation?.MerchantID;
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
    }
}
