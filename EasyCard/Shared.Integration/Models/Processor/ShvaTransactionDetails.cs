using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.Processor
{
    public class ShvaTransactionDetails
    {
        /// <summary>
        ///  Shva Shovar Number
        /// </summary>
        public string ShvaShovarNumber { get; set; }

        /// <summary>
        /// Transmission Date
        /// </summary>
        public DateTime? TransmissionDate { get; set; }
    }
}
