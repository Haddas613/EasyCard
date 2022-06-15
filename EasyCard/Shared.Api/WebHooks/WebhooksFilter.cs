using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Api.WebHooks
{
    public class WebhooksFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }
    }
}
