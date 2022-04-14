using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.WebHooks
{
    public interface IWebHookEntity
    {
        WebHooksConfiguration WebHooksConfiguration { get; set; }
    }
}
