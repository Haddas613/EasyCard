using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.WebHooks
{
    public interface IWebHookService
    {
        Task<OperationResponse> ExecuteWebHook(WebHookData webHookData);
    }
}
