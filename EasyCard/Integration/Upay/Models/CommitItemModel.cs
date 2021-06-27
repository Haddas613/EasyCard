using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    [DataContract]
    public class CommitItemModel : ParameterBase
    {
        /// <summary>
        /// Gets or Sets email
        /// </summary>
        [DataMember(Name = "externalid")]
        public string Externalid { get; set; }

        /// <summary>
        /// Gets or Sets CashierID
        /// </summary>
        [DataMember(Name = "cashierid")]
        public string Cashierid { get; set; }

        /// <summary>
        /// Gets or Sets NumberPayments
        /// </summary>
        [DataMember(Name = "numberpayments", IsRequired = false)]
        public string Numberpayments { get; set; }

        /// <summary>
        /// Gets or Sets Success
        /// </summary>
        [DataMember(Name = "success")]
        public string Success { get; set; }
    }
}
