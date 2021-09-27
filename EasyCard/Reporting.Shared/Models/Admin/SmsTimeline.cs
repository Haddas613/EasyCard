using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Shared.Models.Admin
{
    public class SmsTimeline
    {
        public int? Year { get; set; }

        public SmsTimelineTypeEnum Type { get; set; }

        public object DimensionValue { get; set; }

        public string Dimension
        {
            get
            {
                return DimensionValue is DateTime? ? ((DateTime?)DimensionValue)?.ToString("dd/MM") : $"{Year}-{DimensionValue?.ToString()}";
            }
        }

        public long Measure { get; set; }
    }

    public enum SmsTimelineTypeEnum : short
    {
        Error = -1,
        Success = 0
    }
}
