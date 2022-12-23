using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface IImpersonationService
    {
        Task<Guid?> GetImpersonatedMerchantID(Guid userId);

        Task<OperationResponse> LoginAsMerchant(Guid merchantID);

        Task<OperationResponse> Impersonate(Guid userId, Guid merchantID);
    }
}
