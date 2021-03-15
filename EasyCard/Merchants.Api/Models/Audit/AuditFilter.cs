using Shared.Api.Models;
using Shared.Business.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Audit
{
    public class AuditFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        public string TerminalName { get; set; }

        public Guid? MerchantID { get; set; }

        public string MerchantName { get; set; }

        /// <summary>
        /// MerchantHistory.OperationDoneByID
        /// </summary>
        public Guid? UserID { get; set; }

        public string UserName { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public OperationCodesEnum? Code { get; set; }
    }
}
