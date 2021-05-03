using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Dictionaries
{
    public class MerchantsDictionaries
    {
        public Dictionary<string, string> TerminalStatusEnum { get; set; }

        public Dictionary<string, string> UserStatusEnum { get; set; }

        public Dictionary<string, string> LogLevelsEnum { get; set; }

        public Dictionary<string, string> OperationCodesEnum { get; set; }

        public Dictionary<string, string> TerminalTransmissionScheduleEnum { get; set; }

        public Dictionary<string, string> DateFilterTypeEnum { get; set; }
    }
}
