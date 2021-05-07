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
            log.LogInformation($"Trigger Billing Deals");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var billingDealIDs = JsonConvert.DeserializeObject<IEnumerable<Guid>>(messageBody);

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var request = new TransactionsApi.Models.Billing.CreateTransactionFromBillingDealsRequest
            {
                BillingDealsID = billingDealIDs
            };

            var response = await transactionsApiClient.CreateTransactionsFromBillingDeals(request);

            log.LogInformation($"Trigger Billing Deals Completed. Success:{response.SuccessfulCount}; Failed: {response.FailedCount}; Response {response.Message};");
        }
    }
}
