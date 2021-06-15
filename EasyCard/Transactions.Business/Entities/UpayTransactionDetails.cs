using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class UpayTransactionDetails
    {
        public string CashieriD { get; set; }

        public decimal TotalAmount { get; set; }

        public string CreditCardCompanyCode { get; set; }

        public string MerchantNumber { get; set; }

        public string SessionID { get; set; }

        public string ErrorMessage { get; set; }

        public string WebUrl { get; set; }

        public string ErrorDescription { get; set; }
    }
}
