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
    public static class CardTokensRetentionCleanup
    {
        [FunctionName("CardTokensRetentionCleanup")]
        public static async Task Run(
                   [TimerTrigger("%CardTokensRetentionCleanupTrigger%")] TimerInfo myTimer,
                   ILogger log,
                   ExecutionContext context)
        {
            log.LogInformation($"CardTokensRetentionCleanup");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            try
            {
                var response = await transactionsApiClient.CardTokensRetentionCleanup();
                if (response.Status == SharedApi.Models.Enums.StatusEnum.Success)
                {
                    log.LogInformation($"Successfully performed CardTokensRetentionCleanup: {response.Message}");
                }
                else
                {
                    log.LogError($"Cannot perform CardTokensRetentionCleanup: {response.Message} ({response.AdditionalData})");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Cannot perform CardTokensRetentionCleanup: {ex.Message}");
            }
        }
    }
}
