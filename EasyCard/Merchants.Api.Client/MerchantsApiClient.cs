using Merchants.Api.Client.Models;
using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.Terminal;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SharedApi = Shared.Api;

namespace Merchants.Api.Client
{
    public class MerchantsApiClient : IMerchantsApiClient
    {
        private readonly IWebApiClient webApiClient;
        private readonly ApiSettings apiConfiguration;
        private readonly IWebApiClientTokenService tokenService;

        public MerchantsApiClient(IWebApiClient webApiClient,
            IWebApiClientTokenService tokenService, 
            IOptions<ApiSettings> apiConfiguration)
        {
            this.webApiClient = webApiClient;
            this.apiConfiguration = apiConfiguration.Value;
            this.tokenService = tokenService;
        }

        public NameValueCollection Headers { get; } = new NameValueCollection();

        public async Task<OperationResponse> CreateMerchant(MerchantRequest merchantRequest)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantsManagementApiAddress, "api/merchant", merchantRequest, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger?.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
            }
        }

        public async Task<OperationResponse> CreateTerminal(TerminalRequest terminalRequest)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantsManagementApiAddress, "api/terminals", terminalRequest, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger?.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
            }
        }

        public async Task<SummariesResponse<PlanSummary>> GetPlans()
        {
            try
            {
                return await webApiClient.Get<SummariesResponse<PlanSummary>>(apiConfiguration.MerchantsManagementApiAddress, "api/plans", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger?.LogError(clientError.Message);
                return new SummariesResponse<PlanSummary>();
            }
        }

        public async Task<OperationResponse> LinkUserToMerchant(LinkUserToMerchantRequest request)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantsManagementApiAddress, "api/user/linkToMerchant", request, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger?.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
            }
        }

        public async Task<OperationResponse> LogUserActivity(UserActivityRequest request)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantsManagementApiAddress, "api/user/logActivity", request, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger?.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
            }
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            var token = await tokenService.GetToken();

            NameValueCollection headers = new NameValueCollection(Headers);

            if (token != null)
            {
                headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", token.AccessToken).ToString());
            }

            return headers;
        }

        public async Task<OperationResponse> AuditResetApiKey(Guid terminalID, Guid merchantID)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantsManagementApiAddress, $"api/terminals/{terminalID}/auditResetApiKey/{merchantID}", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger?.LogError(clientError.Message);
                return new OperationResponse("Error", SharedApi.Models.Enums.StatusEnum.Error);
            }
        }

        public async Task<SummariesResponse<TerminalSummary>> GetTerminals(TerminalsFilter filter)
        {
            try
            {
                return await webApiClient.Get<SummariesResponse<TerminalSummary>>(apiConfiguration.MerchantsManagementApiAddress, "api/terminals", filter, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                throw;
                //logger?.LogError(clientError.Message);
                //return new SummariesResponse<TerminalSummary>();
            }
        }

        public async Task<OperationResponse> UpdateTerminalParameters(Guid terminalID)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantsManagementApiAddress, $"api/integrations/shva/update-params", new { TerminalID = terminalID }, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
            }
        }

        public async Task<OperationResponse> Impersonate(string userID, Guid? merchantID)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantsManagementApiAddress, $"api/user/{userID}impersonate/{merchantID}",null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
            }
        }

        public async Task<IEnumerable<MerchantSummary>> GetMerchants(IEnumerable<Guid?> merchantIDs)
        {
            try
            {
                var res = await webApiClient.Get<SummariesResponse<MerchantSummary>>(apiConfiguration.MerchantsManagementApiAddress, "api/merchant", new { merchantIDs }, BuildHeaders);
                return res?.Data;
            }
            catch (WebApiClientErrorException clientError)
            {
                return new List<MerchantSummary>();
            }
            catch (WebApiServerErrorException serverError)
            {
                return new List<MerchantSummary>();
            }
        }
    }
}
