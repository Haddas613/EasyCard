using Shared.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Integration
{
    public class ShvaTerminal : IEntityBase<string>
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string MerchantNumber { get; set; }

        public string GetID()
        {
            return MerchantNumber;
        }
    }
}
