using Merchants.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Api.Client.Models
{
    public class FeatureSummary
    {
        public FeatureEnum FeatureID { get; set; }

        public string NameEN { get; set; }

        public string NameHE { get; set; }

        public decimal? Price { get; set; }

        public byte[] UpdateTimestamp { get; set; }
    }
}
