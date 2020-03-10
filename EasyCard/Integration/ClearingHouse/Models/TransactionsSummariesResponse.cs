using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Transactions Summaries Response
    /// </summary>
    [DataContract]
    public partial class TransactionsSummariesResponse
    {
        /// <summary>
        /// Gets or Sets Data
        /// </summary>
        [DataMember(Name = "data")]
        public List<TransactionSummary> Data { get; set; }

        /// <summary>
        /// Gets or Sets NumberOfRecords
        /// </summary>
        [DataMember(Name = "numberOfRecords")]
        public int? NumberOfRecords { get; set; }
    }
}
