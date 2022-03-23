using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.WebHooks
{
    public class WebHookData
    {
        public Guid? MerchantID { get; set; }

        public Guid? TerminalID { get; set; }

        public string Url { get; set; }

        public object Payload { get; set; }

        public SecurityHeader SecurityHeader { get; set; }

        public string CorrelationId { get; set; }
    }
}
