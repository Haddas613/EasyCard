using System;
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

namespace FunctionsCompositionApp.Invoicing
{
    public static class TransmitTerminalTransactions
    {
        [FunctionName("TransmitTerminalTransactions")]
        public static async Task Run([QueueTrigger("transmission")] string messageBody, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"Transmitting terminal {messageBody} transactions");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var terminalID = Guid.Parse(messageBody);

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var response = await transactionsApiClient.TransmitTerminalTransactions(terminalID);

            if (response.Status == SharedApi.Models.Enums.StatusEnum.Error)
            {
                log.LogError($"{response.Message}; terminalID {messageBody}; CorrelationID: {response.CorrelationId}");
            }
            else
            {
                log.LogInformation(response.Message);
            }
        }
    }
}
