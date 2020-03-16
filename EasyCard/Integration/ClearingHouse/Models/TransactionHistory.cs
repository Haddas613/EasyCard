using System;
using System.Runtime.Serialization;
using System.Text;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Transaction History
    /// </summary>
    [DataContract]
    public partial class TransactionHistory
    {
        /// <summary>
        /// Gets or Sets TransactionID
        /// </summary>
        [DataMember(Name = "transactionID")]
        public long? TransactionID { get; set; }

        /// <summary>
        /// Gets or Sets OperationDate
        /// </summary>
        [DataMember(Name = "operationDate")]
        public DateTime? OperationDate { get; set; }

        /// <summary>
        /// Gets or Sets OperationDoneBy
        /// </summary>
        [DataMember(Name = "operationDoneBy")]
        public string OperationDoneBy { get; set; }

        /// <summary>
        /// Gets or Sets OperationDoneByID
        /// </summary>
        [DataMember(Name = "operationDoneByID")]
        public string OperationDoneByID { get; set; }

        /// <summary>
        /// e.g.: created, commited, rejected, chargedback
        /// </summary>
        [DataMember(Name = "operationCode")]
        public string OperationCode { get; set; }

        /// <summary>
        /// Gets or Sets OperationDescription
        /// </summary>
        [DataMember(Name = "operationDescription")]
        public string OperationDescription { get; set; }
    }
}
