using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Merchant
{
    public class Impersonation
    {
        public Guid UserId { get; set; }

        public Guid MerchantID { get; set; }
    }
}
