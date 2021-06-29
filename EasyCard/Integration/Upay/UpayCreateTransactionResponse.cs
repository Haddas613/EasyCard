using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Upay
{
    public class UpayCreateTransactionResponse : AggregatorCreateTransactionResponse
    {
        public long Cashierid { get; set; }

        public string TotalAmount { get; set; }
        
        public string CreditcardCompanycode { get; set; }

        public string MerchantNumber { get; set; }

        public string SessionId { get; set; }

        public string WebUrl { get; set; }
       
        public string ErrorDescription { get; set; }
    }
}
