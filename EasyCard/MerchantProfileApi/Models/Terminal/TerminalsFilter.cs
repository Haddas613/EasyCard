using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalsFilter : FilterBase
    {
        public string Label { get; set; }
    }
}
