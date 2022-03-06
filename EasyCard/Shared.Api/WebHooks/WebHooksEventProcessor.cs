using Shared.Helpers.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.WebHooks
{
    public class WebHooksEventProcessor : IEventProcessor
    {
        private readonly IWebHookService webHookService;

        public WebHooksEventProcessor(IWebHookService webHookService)
        {
            this.webHookService = webHookService;
        }

        public bool CanProcess(CustomEvent evnt)
        {
            return (evnt.Entity as IWebHookEntity)?.WebHooksConfiguration != null;
        }

        public Task ProcessEvent(CustomEvent evnt)
        {
            var whentity = evnt.Entity as IWebHookEntity;

            var whcfg = whentity.WebHooksConfiguration;

            var webhook = whcfg.WebHooks?.FirstOrDefault(d => d.IsFailureEvent == evnt.IsFailureEvent && d.EventName == evnt.EventName && d.EntityType == evnt.EntityType);

            if (webhook != null)
            {
                return this.webHookService.ExecuteWebHook(new WebHookData
                {
                    MerchantID = evnt.MerchantID,
                    TerminalID = evnt.TerminalID,
                    CorrelationId = evnt.CorrelationId,
                    Payload = evnt,
                    SecurityHeader = whcfg.SecurityHeader,
                    Url = webhook.Url
                });
            }

            return Task.FromResult(true);
        }
    }
}
