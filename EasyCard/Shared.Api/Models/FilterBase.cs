using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Api.Models
{
    public class FilterBase
    {
        public int? Take { get; set; }

        public int? Skip { get; set; }

        public string OrderBy { get; set; }

        public OrderByTypeEnum OrderByType { get; set; }
    }
}
