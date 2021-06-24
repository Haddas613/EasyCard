using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    public class CreateTranModel : ParameterBase
    {
        /// <summary>
        /// Gets or Sets transfers
        /// </summary>
        [DataMember(Name = "transfers")]
        public TransfersModel[] Transfers { get; set; }

        /// <summary>
        /// Gets or Sets passwordmd5
        /// </summary>
        [DataMember(Name = "passwordmd5")]
        public string Passwordmd5 { get; set; }

        /// <summary>
        /// Gets or Sets key
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets cardreader
        /// </summary>
        [DataMember(Name = "cardreader")]
        public string Cardreader { get; set; }

        /// <summary>
        /// Gets or Sets creditcardcompanytype
        /// </summary>
        [DataMember(Name = "creditcardcompanytype")]
        public string Creditcardcompanytype { get; set; }

        /// <summary>
        /// Gets or Sets creditcardtype
        /// </summary>
        [DataMember(Name = "creditcardtype")]
        public string Creditcardtype { get; set; }

        /// <summary>
        /// Gets or Sets providername
        /// </summary>
        [DataMember(Name = "providername")]
        public string Providername { get; set; }
    }
}
