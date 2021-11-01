using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Integration.Models
{
    public class Address
    {
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string CountryCode { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string City { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string Zip { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(250)]
        public string Street { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string House { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string Apartment { get; set; }
    }
}
