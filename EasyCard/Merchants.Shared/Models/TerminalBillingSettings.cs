using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Models
{
    public class TerminalBillingSettings
    {
        // TODO: validation
        public IEnumerable<string> BillingNotificationsEmails { get; set; }

        /// <summary>
        /// If it is true, each next recurrent payment, configured by billing deal schedule, will be created automatically
        /// </summary>
        public bool? CreateRecurrentPaymentsAutomatically { get; set; }

        /// <summary>
        /// After given number of transactions billing will be inactive
        /// </summary>
        public int? FailedTransactionsCountBeforeInactivate { get; set; }

        /// <summary>
        /// Transaction creation will be retried after N days
        /// </summary>
        public int? NumberOfDaysToRetryTransaction { get; set; }
    }
}
