using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.Merchant
{
    public class MerchantRequest
    {
        public string MerchantID { get; set; }

        [Required]
        public string BusinessName { get; set; }
    }
}
