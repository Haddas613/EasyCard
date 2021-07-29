using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models
{
    [DataContract]
    public class ExpirationModel
    {
        [DataMember(Name = "month")]
        public int Month { get; set; }

        [DataMember(Name = "year")]
        public int Year { get; set; }
    }
}
