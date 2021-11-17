using BankOfIsrael;
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
using Transactions.Api.Models.Currency;
using SharedHelpers = Shared.Helpers;
using SharedApi = Shared.Api;

namespace FunctionsCompositionApp.UpdateParameters
{
    public static class UpdateCurrencyRates
    {
        [FunctionName("UpdateCurrencyRates")]
        public static async Task Run(
            [TimerTrigger("%UpdateCurrencyRatesTrigger%")]TimerInfo myTimer,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"{nameof(UpdateCurrencyRates)}  at: {DateTime.Now}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var section = config.GetSection("BankOfIsrael");
            log.LogInformation($"Section {section?.Value}");

            var boiUrl = section.GetValue<string>("URL");

            var plugin = new CurrenciesRateBankPlugin(boiUrl);

            var rates = plugin.GetRates();

            foreach (var rate in rates.Rates.Where(d => d.Currency == "USD" || d.Currency == "EUR"))
            {

                var request = new CurrencyRateUpdateRequest
                {
                    Currency = Enum.Parse<SharedHelpers.CurrencyEnum>(rate.Currency),
                    Rate = rate.Rate,
                    Date = rate.Date
                };

                log.LogInformation($"Starting UpdateCurrencyRatesFunction for {rate.Currency} and {request.Date}");

                var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

                var response = await transactionsApiClient.UpdateCurrencyRates(request);

                if (response.Status == SharedApi.Models.Enums.StatusEnum.Success)
                {
                    log.LogInformation($"UpdateCurrencyRates for {rate.Currency} success: {response.Status}");
                }
                else
                {
                    log.LogError($"UpdateCurrencyRates for {rate.Currency} failed: {response.Status}, {response.Message}");
                }
            }

            log.LogInformation($"UpdateCurrencyRates completed");
        }
    }
}
