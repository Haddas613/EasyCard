using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class NayaxResult
    {
        public bool Approval { get; set; }
        public string ResultText { get; set; }
        public string Mutav { get; set; }
        public string Vuid { get; set; }
        public string SysTraceNumber { get; set; }
        public string CorrelationID { get; set; }
        public string UpdateReceiptNumber { get; set; }

        public NayaxResult(string message, bool approval)
        {
            this.ResultText = message;
            this.Approval = approval;
        }

        public NayaxResult(string resultText, bool approval, string vuid = "-1", string sysTraceNumber = null, string correlationID = null, string mutav = null)
        {
            this.ResultText = resultText;
            this.Approval = approval;
            this.Vuid = vuid;
            this.SysTraceNumber = sysTraceNumber;
            this.CorrelationID = correlationID;
            this.Mutav = mutav;
        }
    }
}
