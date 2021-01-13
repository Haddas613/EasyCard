using Merchants.Business.Entities.Terminal;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Merchant
{
    public class Plan : IEntityBase<long>
    {
        public long PlanID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public bool Active { get; set; }

        public long TerminalTemplateID { get; set; }

        public TerminalTemplate TerminalTemplate { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public long GetID() => PlanID;
    }
}
