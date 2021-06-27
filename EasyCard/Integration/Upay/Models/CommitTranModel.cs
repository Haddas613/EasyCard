using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    [DataContract]
    public class CommitTranModel : ParameterBase
    {
        /// <summary>
        /// Gets or Sets providername
        /// </summary>
        [DataMember(Name = "providername")]
        public string Providername { get; set; }
        /// <summary>
        /// Gets or Sets email
        /// </summary>
        [DataMember(Name = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or Sets Password
        /// </summary>
        [DataMember(Name = "passwordmd5")]
        public string Passwordmd5 { get; set; }

        /// <summary>
        /// Gets or Sets key
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets transfers
        /// </summary>
        [DataMember(Name = "returntransfers")]
        public CommitItemModel[] Returntransfers { get; set; }

    }
}
