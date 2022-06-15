using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalCheckoutSettingsUpdate
    {
        // TODO: move to other place (sepends on implementation)
        public string CustomCssReference { get; set; }

        // TODO: validation
        public IEnumerable<string> RedirectUrls { get; set; }

        public bool LegacyRedirectResponse { get; set; }

        public string DefaultLanguage { get; set; }

        public bool? AllowInstallments { get; set; }

        public bool? AllowCredit { get; set; }

        public bool? HidePhone { get; set; }

        public bool? HideEmail { get; set; }

        public bool? HideNationalID { get; set; }
    }
}
