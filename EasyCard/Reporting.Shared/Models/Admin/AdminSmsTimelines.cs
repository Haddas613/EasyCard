using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Shared.Models.Admin
{
    public class AdminSmsTimelines
    {
        public IEnumerable<SmsTimeline> Success { get; set; }

        public IEnumerable<SmsTimeline> Error { get; set; }

        public long? SuccessMeasure { get; set; }

        public long? ErrorMeasure { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public ReportGranularityEnum? Granularity { get; set; }
    }
}
