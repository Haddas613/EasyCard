using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    [DataContract]
    public class Error
    {

        [DataMember(Name = "codes")]
        public IList<string> Codes { get; set; }

        [DataMember(Name = "arguments")]
        public IList<Argument> Arguments { get; set; }

        [DataMember(Name = "defaultMessage")]
        public string DefaultMessage { get; set; }

        [DataMember(Name = "objectName")]
        public string ObjectName { get; set; }

        [DataMember(Name = "field")]
        public string Field { get; set; }

        [DataMember(Name = "rejectedValue")]
        public object RejectedValue { get; set; }

        [DataMember(Name = "bindingFailure")]
        public bool BindingFailure { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }
    }
}
