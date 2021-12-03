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

namespace FunctionsCompositionApp.UpdateParameters
{
    public static class SendTerminalsToUpdateParametersQueue
    {
        [FunctionName("SendTerminalsToUpdateParametersQueue")]
        public static async Task Run(
            [TimerTrigger("%SendTerminalsToUpdateParametersTrigger%")]TimerInfo myTimer,
            ILogger log,
            [Queue("updateTerminalSHVAParameters")] IAsyncCollector<string> outputQueue,
            ExecutionContext context)
        {
            log.LogInformation($"{nameof(SendTerminalsToUpdateParametersQueue)}  at: {DateTime.Now}");

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
                Skip = 0,
                Take = 100
            };

            bool fetch = true;
            int processedTerminalCount = 0, totalTerminalsCount = 0;

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
                    await outputQueue.AddAsync(terminal.TerminalID.ToString());
                    log.LogInformation($"Sent {terminal.TerminalID} to updateTerminalSHVAParameters queue");
                }

                filter.Skip += filter.Take;

                var batchSize = terminals.Data.Count();
                processedTerminalCount += batchSize;
                fetch = batchSize > 0;
            }

            log.LogInformation($"Sent {processedTerminalCount} terminals to updateTerminalSHVAParameters queue");
        }
    }
}
