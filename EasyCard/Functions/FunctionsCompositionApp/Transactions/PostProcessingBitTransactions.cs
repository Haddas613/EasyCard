using System;
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
using TransactionsShared = Transactions.Shared;

namespace FunctionsCompositionApp.Transactions
{
    public static class PostProcessingBitTransactions
    {
        [FunctionName("PostProcessingBitTransactions")]
        public static async Task Run([TimerTrigger("%PostProcessingBitTransactionsScheduleTriggerTime%")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var response = await transactionsApiClient.GetTransactions(new TransactionsFilter { DocumentOrigin = TransactionsShared.Enums.DocumentOriginEnum.Bit, QuickStatusFilter = TransactionsShared.Enums.QuickStatusFilterTypeEnum.Pending });

            var toBeProcessed = response.Data.Where(t => t.TransactionTimestamp < DateTime.UtcNow.AddMinutes(-15));
            log.LogInformation($"Should be post-processed {toBeProcessed.Count()} Bit transactions");
            int successCount = 0;
            int failedCount = 0;

            foreach (var transaction in toBeProcessed)
            {
                try
                {
                    var res = await transactionsApiClient.BitTransactionPostProcessing(transaction.PaymentTransactionID);

                    if (res.Status != SharedApi.Models.Enums.StatusEnum.Success)
                    {
                        failedCount++;
                        log.LogError($"Failed to process Bit transaction {transaction.PaymentTransactionID}: {res.Message}");
                    }
                    else
                    {
                        successCount++;
                        log.LogInformation($"Successfully processed Bit transaction {transaction.PaymentTransactionID}: {res.Message}");
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    log.LogError(ex, $"Failed to process Bit transaction {transaction.PaymentTransactionID}: {ex.Message}");
                }
            }

            log.LogInformation($"Post-processed {successCount} Bit transactions; failed {failedCount} Bit transactions");
        }
    }
}
