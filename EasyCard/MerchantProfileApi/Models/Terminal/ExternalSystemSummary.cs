using Merchants.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class ExternalSystemSummary
    {
        public long ExternalSystemID { get; set; }

        public ExternalSystemTypeEnum Type { get; set; }

        public string Name { get; set; }
    }
}
