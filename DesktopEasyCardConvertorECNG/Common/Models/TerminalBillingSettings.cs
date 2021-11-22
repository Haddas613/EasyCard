using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class TerminalBillingSettings
    {
        // TODO: validation
        public IEnumerable<string> BillingNotificationsEmails { get; set; }

        /// <summary>
        /// If it is true, each next recurrent payment, configured by billing deal schedule, will be created automatically
        /// </summary>
        public bool? CreateRecurrentPaymentsAutomatically { get; set; }
    }
}
