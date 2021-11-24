using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var merchantsApiClient = MerchantsApiClientHelper.GetMerchantsApiClient(log, config);

            var filter = new Merchants.Api.Models.Terminal.TerminalsFilter
            {
                ActiveOnly = true,
                HasFeature = Merchants.Shared.Enums.FeatureEnum.Billing,
                Skip = 0,
                Take = 100
            };

            bool fetch = true;
            int totalBillingDeals = 0, processedTerminalCount = 0, totalTerminalsCount = 0;

            while (fetch)
            {
                var terminals = await merchantsApiClient.GetTerminals(filter);
                if (terminals.NumberOfRecords == 0 || !terminals.Data.Any())
                {
                    log.LogInformation($"No terminals to process");
                    fetch = false;
                    break;
                }

                if (totalTerminalsCount == 0)
                {
                    totalTerminalsCount = terminals.NumberOfRecords;
                }
                log.LogInformation($"Sending {filter.Skip} of {terminals.NumberOfRecords} terminals");

                foreach (var terminal in terminals.Data)
                {
                    log.LogInformation($"Sending billing deals for terminal #{terminal.TerminalID}:{terminal.Label}");
                    var response = await transactionsApiClient.SendBillingDealsToQueue(terminal.TerminalID);
                    totalBillingDeals += response.Count;
                }

                filter.Skip += filter.Take;

                var batchSize = terminals.Data.Count();
                processedTerminalCount += batchSize;
                fetch = batchSize > 0;
            }

            log.LogInformation($"Send {totalBillingDeals} billing deals from {processedTerminalCount} out of total {totalTerminalsCount} terminals billing queue messages");
        }
    }
}
