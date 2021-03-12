using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FunctionsCompositionApp.Transactions
{
    public static class GetTransmissionTerminals
    {
        [FunctionName("GetTransmissionTerminals")]
        public static async Task Run([TimerTrigger("%GetTransmissionTerminalsScheduleTriggerTime%")]TimerInfo myTimer, 
            ILogger log, 
            ExecutionContext context, 
            [Queue("transmission")]IAsyncCollector<string> outputQueue)
        {
            log.LogInformation($"{nameof(GetTransmissionTerminals)}  at: {DateTime.Now}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var terminals = await transactionsApiClient.GetNonTransmittedTransactionsTerminals();

            foreach (var terminal in terminals)
            {
                await outputQueue.AddAsync(terminal.ToString());
            }

            log.LogInformation($"Send {terminals.Count()} transmission queue messages");

        }
    }
}
