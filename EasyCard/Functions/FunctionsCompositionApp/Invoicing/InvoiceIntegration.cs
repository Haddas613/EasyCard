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

            var transactionsApiClient = GetTransactionsApiClient(log, config);

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

        private static ITransactionsApiClient GetTransactionsApiClient(ILogger logger, IConfigurationRoot config)
        {
            var section = config.GetSection("IdentityServerClient");
            logger.LogInformation($"Section {section?.Value}");

            var identitySection = config.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();
            var apiCfgsection = config.GetSection("ApiConfig")?.Get<TransactionsApiClientConfig>();

            logger.LogInformation($"Authority {identitySection?.Authority}");
            logger.LogInformation($"ClientID {identitySection?.ClientID}");

            var cfg = Options.Create(identitySection);
            var apiCfg = Options.Create(apiCfgsection);
            var webApiClient = new WebApiClient();
            var tokenService = new WebApiClientTokenService(webApiClient.HttpClient, cfg);

            return new TransactionsApiClient(webApiClient, /*logger,*/ tokenService, apiCfg);
        }
    }
}
