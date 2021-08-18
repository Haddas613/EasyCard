using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class MerchantName
    {
        public string posName { get; set; }
        public MerchantName(string posName)
        {
            this.posName = posName;
        }
    }
}
