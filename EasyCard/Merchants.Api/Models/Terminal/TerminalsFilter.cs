using Shared.Api.Models;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalsFilter: FilterBase
    {
        public long? TerminalID { get; set; }

        public string Label { get; set; }
    }
}
