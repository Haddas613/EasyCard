using System;
using System.Runtime.Serialization;
using System.Text;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Bank Account
    /// </summary>
    [DataContract]
    public partial class BankAccount
    {
        /// <summary>
        /// Gets or Sets BankNumber
        /// </summary>
        [DataMember(Name = "bankNumber")]
        public string BankNumber { get; set; }

        /// <summary>
        /// Gets or Sets BranchNumber
        /// </summary>
        [DataMember(Name = "branchNumber")]
        public string BranchNumber { get; set; }

        /// <summary>
        /// Gets or Sets BankAccountNumber
        /// </summary>
        [DataMember(Name = "bankAccountNumber")]
        public string BankAccountNumber { get; set; }
    }
}
