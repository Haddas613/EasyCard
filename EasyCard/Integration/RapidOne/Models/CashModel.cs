using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOne.Models
{
    [DataContract]
    public class CashModel
    {
        [DataMember(Name = "value")]
        public decimal Value { get; set; }
    }
}
