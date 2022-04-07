using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Models
{
    /// <summary>
    /// Settings for checkout (redirect) page
    /// </summary>
    public class TerminalCheckoutSettings
    {
        // TODO: move to other place (sepends on implementation)
        public string CustomCssReference { get; set; }

        public bool? IssueInvoice { get; set; }

        // TODO: validation
        public IEnumerable<string> RedirectUrls { get; set; }

        public bool LegacyRedirectResponse { get; set; }

        /// <summary>
        /// If 3DSecure raises error - continue flow without 3ds
        /// </summary>
        public bool? ContinueInCaseOf3DSecureError { get; set; }

        public string DefaultLanguage { get; set; }
    }
}
