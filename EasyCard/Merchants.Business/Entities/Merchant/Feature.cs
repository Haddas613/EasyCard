using Merchants.Shared.Enums;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Merchant
{
    public class Feature : IEntityBase<FeatureEnum>
    {
        public FeatureEnum FeatureID { get; set; }

        public string NameEN { get; set; }

        public string NameHE { get; set; }

        public string DescriptionEN { get; set; }

        public string DescriptionHE { get; set; }

        public decimal? Price { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public FeatureEnum GetID() => FeatureID;
    }
}
