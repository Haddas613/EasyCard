﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Helpers;
using Shared.Helpers.Security;
using Transactions.Api.Client;
using Transactions.Api.Models.Transactions;
using SharedApi = Shared.Api;
using SharedIntegration = Shared.Integration;

namespace FunctionsCompositionApp.Transactions
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

            var request = JsonConvert.DeserializeObject<TransmitTransactionsRequest>(messageBody);

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var response = await transactionsApiClient.TransmitTransactions(request);

            var totalCount = response.Data.Count();
            var failedCount = response.Data.Where(t => t.TransmissionStatus != SharedIntegration.Models.TransmissionStatusEnum.Transmitted).Count();

            if (failedCount > 0)
            {
                log.LogError($"Failed Transactions: {failedCount}; Successful Transactions: {totalCount - failedCount}; terminalID {messageBody};");
            }
            else
            {
                log.LogInformation($"Successful Transactions: {totalCount}; terminalID {messageBody};");
            }
        }
    }
}
