using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.ClearingHouse
{
    public class ClearingHouseCustomerResponse
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string MerchantReference { get; set; }

        public string ShvaTerminalReference { get; set; }
    }
}
