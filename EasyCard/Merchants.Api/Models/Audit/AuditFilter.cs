using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Audit
{
    public class AuditFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        public Guid? MerchantID { get; set; }

        /// <summary>
        /// MerchantHistory.OperationDoneByID
        /// </summary>
        public Guid? UserID { get; set; }
    }
}
