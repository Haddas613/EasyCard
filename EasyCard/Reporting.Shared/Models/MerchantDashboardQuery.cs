using Newtonsoft.Json;
using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Reporting.Shared.Models
{
    public class MerchantDashboardQuery
    {
        [Required]
        public Guid? TerminalID { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public DateTime? TimelineDateFrom { get; set; }

        public DateTime? TimelineDateTo { get; set; }

        public QuickDateFilterTypeEnum? QuickDateFilter { get; set; }

        [JsonIgnore]
        public Guid? MerchantID { get; set; }
    }
}
