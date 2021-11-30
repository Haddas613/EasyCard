using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Common.Models
{
    public class Address
    {
        [StringLength(50)]
        public string CountryCode { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string Zip { get; set; }

        [StringLength(250)]
        public string Street { get; set; }

        [StringLength(50)]
        public string House { get; set; }

        [StringLength(50)]
        public string Apartment { get; set; }
    }
}
