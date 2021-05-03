using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EasyInvoice.Models
{
    [DataContract]
    public class ECCreateCustomerRequest
    {
        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "taxId")]
        public string TaxID { get; set; }
    }
}
