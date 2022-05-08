using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidOne.DTOs.PaymentIntegration
{
    public class CCDealTypeMapping
    {
        public int? CCDealTypeSAPId { get; set; }
        public string PaymentSystemDealType { get; set; }
    }
}
