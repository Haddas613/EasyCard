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
    }
}
