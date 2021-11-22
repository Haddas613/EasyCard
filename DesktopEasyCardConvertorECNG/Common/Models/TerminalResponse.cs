using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
   public  class TerminalResponse
    {
        public MerchantPerTerminalModel merchant { get; set; }
        public string[] enabledFeatures { get; set; }
    }
}
