using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidOne.DTOs.PaymentIntegration
{
    public class CCVendorMapping
    {
        public int? CCSAPId { get; set; }
        public string PaymentSystemVendorName { get; set; }
    }
}
