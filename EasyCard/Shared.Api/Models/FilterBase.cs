using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models
{
    public class FilterBase
    {
        public int? Take { get; set; }

        public int? Skip { get; set; }
    }
}
