using Merchants.Api.Client.Models;
using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Api.Client
{
    public interface IMerchantsApiClient
    {
        Task<OperationResponse> CreateMerchant(MerchantRequest merchantRequest);

        Task<OperationResponse> CreateTerminal(TerminalRequest terminalRequest);

        Task<OperationResponse> LinkUserToMerchant(LinkUserToMerchantRequest request);
    }
}
