using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.RapidOne
{
    public class DepartmentSummary
    {
        public int? DepartmentID { get; set; }

        public int? BranchID { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }
    }
}
