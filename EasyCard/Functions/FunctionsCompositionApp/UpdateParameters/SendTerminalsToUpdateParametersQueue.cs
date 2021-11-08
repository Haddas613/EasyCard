using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctionsCompositionApp.UpdateParameters
{
    public static class SendTerminalsToUpdateParametersQueue
    {
        [FunctionName("SendTerminalsToUpdateParametersQueue")]
        public static async Task Run(
            [TimerTrigger("%SendTerminalsToUpdateParametersTrigger%")]TimerInfo myTimer,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"{nameof(SendTerminalsToUpdateParametersQueue)}  at: {DateTime.Now}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var response = await transactionsApiClient.SendTerminalsToUpdateParametersQueue();

            log.LogInformation($"Sent {response.Count} update SHVA terminal parameters queue messages");
        }
    }
}
