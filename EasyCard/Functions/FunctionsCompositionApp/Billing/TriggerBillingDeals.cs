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
using TransactionsApi = Transactions.Api;

namespace FunctionsCompositionApp.Billing
{
    public static class TriggerBillingDeals
    {
        [FunctionName("TriggerBillingDeals")]
        public static async Task Run([QueueTrigger("billingdeals")] string messageBody, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"Trigger Billing Deals: {messageBody}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var billingDealIDs = JsonConvert.DeserializeObject<IEnumerable<Guid>>(messageBody);

            if (billingDealIDs?.Count() > 0)
            {
                var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

                var request = new TransactionsApi.Models.Billing.CreateTransactionFromBillingDealsRequest
                {
                    BillingDealsID = billingDealIDs
                };

                var response = await transactionsApiClient.CreateTransactionsFromBillingDeals(request);

                if(response.FailedCount > 0)
                {
                    log.LogError($"Trigger Billing Deals Completed. Success:{response.SuccessfulCount}; Failed: {response.FailedCount}; Response {response.Message};");
                }
                else if (response.Errors?.Count() > 0)
                {
                    log.LogError($"Trigger Billing Deals Completed with Errors. Success:{response.SuccessfulCount}; Failed: {response.FailedCount}; Response {response.Message}; Errors: {string.Join("; ", response.Errors.Select(e => $"{e.Code}:{e.Description}"))}");
                }
                else
                {
                    log.LogInformation($"Trigger Billing Deals Completed. Success:{response.SuccessfulCount}; Failed: 0; Response {response.Message};");
                }
            }
            else
            {
                log.LogInformation("Trigger Billing Deals Completed. Nothing to process");
            }
        }
    }
}
