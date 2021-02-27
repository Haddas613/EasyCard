using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using Shared.Helpers.Security;
using Transactions.Api.Client;

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
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var invoiceID = Guid.Parse(messageBody);

            var transactionsApiClient = GetTransactionsApiClient(log, config);

            var response = await transactionsApiClient.GenerateInvoice(invoiceID);

            if (response.Status == Shared.Api.Models.Enums.StatusEnum.Error)
            {
                log.LogError(response.Message);
            }
            else
            {
                log.LogInformation(response.Message);
            }
        }

        private static ITransactionsApiClient GetTransactionsApiClient(ILogger logger, IConfigurationRoot config)
        {
            var identitySection = config.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();
            var apiCfgsection = config.GetSection("ApiConfig")?.Get<TransactionsApiClientConfig>();

            var cfg = Options.Create(identitySection);
            var apiCfg = Options.Create(apiCfgsection);
            var webApiClient = new WebApiClient();
            var tokenService = new WebApiClientTokenService(webApiClient.HttpClient, cfg);

            return new TransactionsApiClient(webApiClient, /*logger,*/ tokenService, apiCfg);
        }
    }
}
