using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalBillingSettingsUpdate
    {
        // TODO: validation
        public IEnumerable<string> BillingNotificationsEmails { get; set; }

        public bool CreateRecurrentPaymentsAutomatically { get; set; }
    }
}
