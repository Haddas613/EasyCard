using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    public class MerchantsFilter : FilterBase
    {
        public string Search { get; set; }
    }
}
