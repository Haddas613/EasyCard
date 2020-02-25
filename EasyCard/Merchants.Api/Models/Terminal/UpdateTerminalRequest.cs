using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class UpdateTerminalRequest
    {
        public long TerminalID { get; set; }
        public string Label { get; set; }
    }
}
