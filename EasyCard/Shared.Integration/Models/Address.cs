using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Integration.Models
{
    public class Address
    {
        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string Zip { get; set; }

        [StringLength(250)]
        public string Street { get; set; }
    }
}
