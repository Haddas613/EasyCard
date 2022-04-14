using Shared.Helpers.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.WebHooks
{
    public class WebHooksConfiguration
    {
        public SecurityHeader SecurityHeader { get; set; }

        public IEnumerable<WebHookConfiguration> WebHooks { get; set; }
    }

    public class WebHookConfiguration
    {
        public string EventName { get; set; }

        public string EntityType { get; set; }

        public bool IsFailureEvent { get; set; }

        public string Url { get; set; }
    }
}
