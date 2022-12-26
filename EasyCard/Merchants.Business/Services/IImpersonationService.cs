using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface IImpersonationService
    {
        Task<OperationResponse> LoginAsMerchant(Guid merchantID);

        Task<OperationResponse> Impersonate(Guid userId, Guid merchantID);

        public Task SetImpersonationClaims(ClaimsPrincipal principal);
    }
}
