using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalCheckoutSettingsUpdate
    {
        // TODO: validation
        public IEnumerable<string> RedurectUrls { get; set; }
    }
}
