using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Api.Models
{
    public class FilterBase
    {
        [Range(-1, double.MaxValue)]
        public int? Take { get; set; }

        [Range(0, double.MaxValue)]
        public int? Skip { get; set; }

        public string SortBy { get; set; }

        // TODO: check with UI
        public OrderByDirectionEnum OrderByDirection { get; set; }
    }
}
