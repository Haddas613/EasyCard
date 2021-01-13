using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class PlanSummary
    {
        public long PlanID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public long TerminalTemplateID { get; set; }

        public ICollection<FeatureSummary> Features { get; set; }
    }
}
