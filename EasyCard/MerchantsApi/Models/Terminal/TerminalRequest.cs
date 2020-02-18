using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.Terminal
{
    public class TerminalRequest
    {
        public long MerchantID { get; set; }
        public string Label { get; set; }
    }
}
