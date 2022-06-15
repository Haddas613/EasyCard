using Shared.Api.Models;
using Shared.Api.WebHooks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.WebHooks
{
    public interface IWebHookService
    {
        Task<OperationResponse> ExecuteWebHook(WebHookData webHookData);

        Task<IEnumerable<ExecutedWebhookSummary>> GetWebHooks(WebhooksFilter filter);
    }
}
