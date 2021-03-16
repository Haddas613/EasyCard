﻿using Newtonsoft.Json;
using Shared.Api.Extensions.Filtering;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Transactions.Shared.Enums;

namespace Reporting.Shared.Models
{
    public class MerchantDashboardQuery
    {
        [Required]
        public Guid? TerminalID { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        public QuickDateFilterTypeEnum? QuickDateFilter { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AltDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AltDateTo { get; set; }

        public QuickDateFilterAltEnum? AltQuickDateFilter { get; set; }

        public ReportGranularityEnum? Granularity { get; set; }
    }
}