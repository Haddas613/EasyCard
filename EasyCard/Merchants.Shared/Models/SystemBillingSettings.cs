using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Models
{
    public class SystemBillingSettings
    {
        /// <summary>
        /// If it is true, each next recurrent payment, configured by billing deal schedule, will be created automatically
        /// </summary>
        public bool? CreateRecurrentPaymentsAutomatically { get; set; }

        /// <summary>
        /// After given number of transactions billing will be inactive
        /// </summary>
        public int? FailedTransactionsCountBeforeInactivate { get; set; } = 5;

        /// <summary>
        /// Transaction creation will be retried after N days
        /// </summary>
        public int? NumberOfDayesToRetryTransaction { get; set; } = 1;
    }
}
