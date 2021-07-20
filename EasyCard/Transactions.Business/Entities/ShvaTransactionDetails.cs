using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class ShvaTransactionDetails
    {
        /// <summary>
        ///  Shva Shovar Number
        /// </summary>
        public string ShvaShovarNumber { get; set; }

        /// <summary>
        /// Shva Deal ID
        /// </summary>
        public string ShvaDealID { get; set; }

        /// <summary>
        ///  Shva Transmission Number
        /// </summary>
        public string ShvaTransmissionNumber { get; set; }

        /// <summary>
        /// Terminal reference
        /// </summary>
        public string ShvaTerminalID { get; set; }

        /// <summary>
        /// Timestamp when transaction has been transmitted to Shva
        /// </summary>
        public DateTime? TransmissionDate { get; set; }

        /// <summary>
        /// in case if DoNotTransmit flag is set, transaction should be transmitted manually (or can be transmitted manually before sheduled period)
        /// </summary>
        public bool ManuallyTransmitted { get; set; }

        /// <summary>
        /// Solek
        /// </summary>
        public SolekEnum? Solek { get; set; }

        /// <summary>
        /// Binary reference
        /// </summary>
        public string TranRecord { get; set; }

        public string ShvaAuthNum { get; set; }

        public string EmvSoftVersion { get; set; }

        public string CompRetailerNum { get; set; }

        public string TelToGetAuthNum { get; set; }
    }
}
