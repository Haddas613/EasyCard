using Newtonsoft.Json;
using Shared.Api.Extensions.Filtering;
using Shared.Api.Models.Enums;
using Shared.Helpers;
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

        public void SetDefault()
        {
            // TODO: dateTo
            if (QuickDateFilter.HasValue)
            {
                DateFrom = CommonFiltertingExtensions.QuickDateToDateTime(QuickDateFilter.Value).Date;
            }

            if (DateFrom == null)
            {
                DateFrom = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
            }

            if (DateTo == null)
            {
                DateTo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
            }

            if (TimelineDateTo == null)
            {
                TimelineDateTo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
            }

            if (TimelineDateFrom == null)
            {
                TimelineDateFrom = TimelineDateTo.Value.AddDays(-30).Date;
            }
        }
    }
}
