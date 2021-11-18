using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class TerminalCheckoutSettings
    {
        // TODO: move to other place (sepends on implementation)
        public string CustomCssReference { get; set; }

        public bool? IssueInvoice { get; set; }

        // TODO: validation
        public IEnumerable<string> RedirectUrls { get; set; }

        public bool LegacyRedirectResponse { get; set; }
    }

}
