using System;
using System.Threading.Tasks;
using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Helpers;
using Shared.Helpers.Security;
using Transactions.Api.Client;
using SharedApi = Shared.Api;

namespace FunctionsCompositionApp.Invoicing
{
    public static class InvoiceIntegration
    {
        [FunctionName("InvoiceIntegration")]
        public static async Task Run([QueueTrigger("invoice")] string messageBody, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"Generating invoice {messageBody}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var invoiceID = Guid.Parse(messageBody);

            var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

            var response = await transactionsApiClient.GenerateInvoice(invoiceID);

            if (response.Status == SharedApi.Models.Enums.StatusEnum.Error)
            {
                log.LogError($"{response.Message}; InvoiceID {messageBody}; CorrelationID: {response.CorrelationId}");
            }
            else
            {
                log.LogInformation(response.Message);
            }
        }
    }
}
