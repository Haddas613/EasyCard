using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.RapidOne
{
    public class CompanySummary
    {
        public string Name { get; set; }

        /// <summary>
        /// DbName in R1
        /// </summary>
        public string DbName { get; set; }
    }
}
