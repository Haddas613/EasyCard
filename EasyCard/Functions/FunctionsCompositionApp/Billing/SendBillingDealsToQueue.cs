using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctionsCompositionApp.Billing
{
    public static class SendBillingDealsToQueue
    {
        [FunctionName("SendBillingDealsToQueue")]
        public static async Task Run(
            [TimerTrigger("%SendBillingDealsToQueueScheduleTriggerTime%")]TimerInfo myTimer,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"{nameof(SendBillingDealsToQueue)}  at: {DateTime.Now}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var response = await transactionsApiClient.SendBillingDealsToQueue();

            log.LogInformation($"Send {response.Count} billing queue messages");
        }
    }
}
