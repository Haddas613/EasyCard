using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantProfileApi.Models.Terminal
{
    public class FeatureResponse
    {
        public long FeatureID { get; set; }

        public string NameEN { get; set; }

        public string NameHE { get; set; }

        public decimal Price { get; set; }

        public string FeatureCode { get; set; }
    }
}
