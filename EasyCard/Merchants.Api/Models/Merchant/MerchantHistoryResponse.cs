using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    public class MerchantHistoryResponse
    {
        public long MerchantHistoryID { get; set; }

        public DateTime OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public string OperationDoneByID { get; set; }

        public string OperationCode { get; set; }

        public string OperationDescription { get; set; }

        public string SourceIP { get; set; }

        public string ReasonForChange { get; set; }
    }
}
