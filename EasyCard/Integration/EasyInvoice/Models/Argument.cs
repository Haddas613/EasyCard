using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EasyInvoice.Models
{
    [DataContract]
    public class Argument
    {
        [DataMember(Name = "codes")]
        public IList<string> Codes { get; set; }

        [DataMember(Name = "arguments")]
        public object Arguments { get; set; }

        [DataMember(Name = "defaultMessage")]
        public string DefaultMessage { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }
    }
}
