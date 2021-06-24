using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    public class MsgModel
    {
        /// <summary>
        /// Gets or Sets  Header
        /// </summary>
        [DataMember(Name = "header")]
        public HeaderBase Header { get; set; }

        /// <summary>
        /// Gets or Sets  Request
        /// </summary>
        [DataMember(Name = "request")]
        public RequestModel Request { get; set; }
    }
}
