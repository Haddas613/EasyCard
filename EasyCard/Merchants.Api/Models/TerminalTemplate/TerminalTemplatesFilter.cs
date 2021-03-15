using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.TerminalTemplate
{
    public class TerminalTemplatesFilter : FilterBase
    {
        public string Label { get; set; }

        public bool? Active { get; set; }
    }
}
