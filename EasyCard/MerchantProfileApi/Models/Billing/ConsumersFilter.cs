using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ConsumersFilter : FilterBase
    {
        public string Search { get; set; }

        public Guid? ConsumerID { get; set; }

        public string ConsumersID { get; set; }
    }
}
