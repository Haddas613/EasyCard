using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOne.Models
{
    [DataContract]
    public class BankDetailsModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "countryCode")]
        public string CountryCode { get; set; }
        public BankDetailsModel()
        {
            this.CountryCode = "IL";
        }
    }
}
