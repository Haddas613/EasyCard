using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Dictionaries
{
    public class MerchantDictionaries
    {
        public Dictionary<string, string> TerminalStatusEnum { get; set; }

        public Dictionary<string, string> TerminalTransmissionScheduleEnum { get; set; }
    }
}
