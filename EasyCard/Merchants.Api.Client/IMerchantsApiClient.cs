using Merchants.Api.Client.Models;
using Merchants.Api.Models.Terminal;
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

        Task<SummariesResponse<PlanSummary>> GetPlans();

        Task<SummariesResponse<TerminalSummary>> GetTerminals(TerminalsFilter filter);

        Task<OperationResponse> LogUserActivity(UserActivityRequest request);

        Task<OperationResponse> AuditResetApiKey(Guid terminalID, Guid merchantID);

        Task<OperationResponse> UpdateTerminalParameters(Guid terminalID);
    }
}
