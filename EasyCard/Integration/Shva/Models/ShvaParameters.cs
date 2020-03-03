using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    public class ShvaParameters
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string MerchantNumber { get; set; }
        /// <summary>
        /// per Transact parameters
        /// </summary>
        public string AuthNum { get; set; }

        public bool IsNewInitDeal { get; set; }
        public InitDealResultModel InitDealModel { get; set; }


    }
}
