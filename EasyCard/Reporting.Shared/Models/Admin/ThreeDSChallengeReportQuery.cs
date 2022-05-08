using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Shared.Models.Admin
{
    public class ThreeDSChallengeReportQuery : FilterBase
    {
        public Guid? MerchantID { get; set; }

        public Guid? TerminalID { get; set; }
    }
}
