﻿using Merchants.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
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
