using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionsApi = Transactions.Api;
using SharedApi = Shared.Api;

namespace FunctionsCompositionApp.UpdateParameters
{
    public static class UpdateTerminalParameters
    {
        [FunctionName("UpdateTerminalParameters")]
        public static async Task Run([QueueTrigger("updateTerminalSHVAParameters")] string messageBody, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"Update Terminal Parameters: {messageBody}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var updateParametersTerminalID = Guid.Parse(messageBody);
            var merchantsApiClient = MerchantsApiClientHelper.GetMerchantsApiClient(log, config);

            try
            {

                var response = await merchantsApiClient.UpdateTerminalParameters(updateParametersTerminalID);
                if (response.Status == SharedApi.Models.Enums.StatusEnum.Success)
                {
                    log.LogInformation($"Updated terminal parameters for terminal {messageBody}");
                }
                else
                {
                    log.LogError($"Cannot update terminal parameters for terminal {messageBody}");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Cannot update terminal parameters for terminal {messageBody}: {ex.Message}");
            }
        }
    }
}
