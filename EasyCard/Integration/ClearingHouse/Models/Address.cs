using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Address
    /// </summary>
    [DataContract]
    public class Address
    {
        /// <summary>
        /// Bank Number
        /// </summary>
        [DataMember(Name = "city")]
        public string City { get; set; }

        /// <summary>
        /// Branch Number
        /// </summary>
        [DataMember(Name = "zip")]
        public string Zip { get; set; }

        /// <summary>
        /// Bank Account Number
        /// </summary>
        [DataMember(Name = "street")]
        public string Street { get; set; }
    }
}
