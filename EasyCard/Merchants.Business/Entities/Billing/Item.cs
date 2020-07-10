using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Billing
{
    public class Item
    {
        public Guid ItemID { get; set; }

        public Guid MerchantID { get; set; }

        public bool Active { get; set; }
    }
}
