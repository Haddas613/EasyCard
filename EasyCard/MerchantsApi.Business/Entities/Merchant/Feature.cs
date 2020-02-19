using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantsApi.Business.Entities
{
    public class Feature
    {
        public long FeatureID { get; set; }
        public string NameEN { get; set; }
        public string NameHE { get; set; }
        public decimal? Price { get; set; }
        public string FeatureCode { get; set; }
        public byte[] UpdateTimestamp { get; set; }
    }
}
