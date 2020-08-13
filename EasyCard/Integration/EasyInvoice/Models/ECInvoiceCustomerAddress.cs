using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EasyInvoice.Models
{
    [DataContract]
    public class ECInvoiceCustomerAddress
    {
        [DataMember(Name = "street")]
        public string Street { get; set; }

        [DataMember(Name = "streetNumber")]
        public string StreetNumber { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        [DataMember(Name = "postalCode")]
        public string PostalCode { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "countryCode")]
        public string CountryCode { get; set; }
    }
}
