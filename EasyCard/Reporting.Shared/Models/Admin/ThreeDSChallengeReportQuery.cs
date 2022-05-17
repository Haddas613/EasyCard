using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Reporting.Shared.Models.Admin
{
    public class ThreeDSChallengeReportQuery : FilterBase
    {
        public Guid? MerchantID { get; set; }

        public Guid? TerminalID { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }
    }
}
