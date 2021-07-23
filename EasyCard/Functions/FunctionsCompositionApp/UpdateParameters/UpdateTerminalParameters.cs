using FunctionsCompositionApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionsApi = Transactions.Api;

namespace FunctionsCompositionApp.UpdateParameters
{
    public static class UpdateTerminalParameters
    {
        [FunctionName("UpdateTerminalParameters")]
        public static async Task Run([QueueTrigger("updateTerminalSHVAParameters")] string messageBody, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"Update Terminal Parameters: {messageBody}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("host.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var updateParametersTerminalIDs = JsonConvert.DeserializeObject<IEnumerable<Guid>>(messageBody);

            if (updateParametersTerminalIDs?.Count() > 0)
            {
                var transactionsApiClient = TransactionsApiClientHelper.GetTransactionsApiClient(log, config);

                var result = Enum.GetValues(typeof(UpdateParamsStatusEnum))
                    .Cast<UpdateParamsStatusEnum>()
                    .ToDictionary(k => k, v => 0);

                foreach (var terminal in updateParametersTerminalIDs)
                {
                    var response = await transactionsApiClient.UpdateTerminalParameters(terminal);

                    result[response.UpdateStatus]++;
                }

                if(result[UpdateParamsStatusEnum.Updated] == 0)
                {
                    log.LogError($"Update Terminal Parameters Completed. Updated: 0;" +
                        $"UpdateFailed: {result[UpdateParamsStatusEnum.UpdateFailed]}; NotFoundOrInvalidStatus: {result[UpdateParamsStatusEnum.NotFoundOrInvalidStatus]};");
                }
                else if(result[UpdateParamsStatusEnum.NotFoundOrInvalidStatus] > 0 || result[UpdateParamsStatusEnum.UpdateFailed] > 0)
                {
                    log.LogWarning($"Update Terminal Parameters Completed. Updated: {result[UpdateParamsStatusEnum.Updated]};" +
                        $"UpdateFailed: {result[UpdateParamsStatusEnum.UpdateFailed]}; NotFoundOrInvalidStatus: {result[UpdateParamsStatusEnum.NotFoundOrInvalidStatus]};");
                }
                else
                {
                    log.LogInformation($"Update Terminal Parameters Completed. Updated: {result[UpdateParamsStatusEnum.Updated]};" +
                        $"UpdateFailed: {result[UpdateParamsStatusEnum.UpdateFailed]}; NotFoundOrInvalidStatus: {result[UpdateParamsStatusEnum.NotFoundOrInvalidStatus]};");
                }
            }
            else
            {
                log.LogInformation("Update Terminal Parameters Completed. Nothing to process");
            }
        }
    }
}
