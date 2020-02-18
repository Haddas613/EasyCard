using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.Terminal
{
    public class GetTerminalsFilter: FilterBase
    {
        public long? TerminalID { get; set; }

        public string Label { get; set; }
    }
}
