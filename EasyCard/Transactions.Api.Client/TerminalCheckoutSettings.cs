using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Api.Client
{
    /// <summary>
    /// Settings for checkout (redirect) page
    /// </summary>
    public class TerminalCheckoutSettings
    {
        // TODO: move to other place (sepends on implementation)
        public string CustomCssReference { get; set; }

        // TODO: validation
        public IEnumerable<string> RedirectUrls { get; set; }
    }
}
