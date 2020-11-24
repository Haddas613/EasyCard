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
    }
}
