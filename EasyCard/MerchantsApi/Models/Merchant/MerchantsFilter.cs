using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.Merchant
{
    public class MerchantsFilter: FilterBase
    {
        public string BusinessName { get; set; }
    }
}
