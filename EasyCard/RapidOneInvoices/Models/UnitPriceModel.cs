using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models
{
    [DataContract]
    public class UnitPriceModel
    {
        [DataMember(Name = "value")]
        public decimal Value { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        public UnitPriceModel()
        {
            this.Currency = "₪";
        }
    }

}
