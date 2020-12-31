using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.TerminalTemplate
{
    public class TerminalTemplatesSummary
    {
        public long TerminalTemplateID { get; set; }

        public string Label { get; set; }

        public DateTime? Created { get; set; }
    }
}
