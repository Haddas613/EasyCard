using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalBillingSettings
    {
        public IEnumerable<string> BillingNotificationsEmails { get; set; }
    }
}
