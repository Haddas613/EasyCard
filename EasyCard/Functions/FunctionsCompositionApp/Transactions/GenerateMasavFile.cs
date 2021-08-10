using System;
using System.Linq;
using System.Threading.Tasks;
using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using Shared.Helpers.Security;
using Transactions.Api.Client;
using SharedApi = Shared.Api;
using SharedIntegration = Shared.Integration;

namespace FunctionsCompositionApp.Transactions
{
    public static class GenerateMasavFile
    {
        [FunctionName("GenerateMasavFile")]
        public static async Task Run([TimerTrigger("%GetTransmissionTerminalsScheduleTriggerTime%")]TimerInfo myTimer,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"Generating Masav file");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var response = await transactionsApiClient.GenerateMasavFile();

            if (response.Status == SharedApi.Models.Enums.StatusEnum.Error)
            {
                log.LogError($"Generating Masav file failed: {response.Message} ");
            }
            else if (response.Status == SharedApi.Models.Enums.StatusEnum.Warning)
            {
                log.LogWarning($"Generating Masav file completed with issues: {response.Message}");
            }
            else
            {
                log.LogInformation($"Generating Masav file completed successfully {response.Message}");
            }
        }
    }
}
