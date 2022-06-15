using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.WebHooks
{
    public class ExecutedWebhookSummary
    {
        public Guid? MerchantID { get; set; }

        public Guid? TerminalID { get; set; }

        public string Url { get; set; }

        public object Payload { get; set; }

        public string CorrelationId { get; set; }

        public Guid? EventID { get; set; }
    }
}
