using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    public class EcwidAddressDetails
    {
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string PostalCode { get; set; }
        public string StateOrProvinceCode { get; set; }
        public string StateOrProvinceName { get; set; }
        public string Phone { get; set; }
    }
}
