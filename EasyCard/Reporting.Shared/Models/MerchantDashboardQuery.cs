using Newtonsoft.Json;
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
    public class MerchantDashboardQuery : DashboardQuery
    {
        [Required]
        public Guid? TerminalID { get; set; }
    }
}
