using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalsFilter : FilterBase
    {
        public Guid? MerchantID { get; set; }

        public string Label { get; set; }
    }
}
