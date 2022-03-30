using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Contract
{
    public class Authenticate3DsRequestModel
    {
        public string MerchantNumber { get; set; }

        public string ThreeDSServerTransID { get; set; }

        public string CardNumber { get; set; }

        public CurrencyEnum Currency { get; set; }

        public string NotificationURL { get; set; }

        public string MerchantName { get; set; }

        public BrowserDetails BrowserDetails { get; set; }

        public decimal? Amount { get; set; }

        public CardExpiration CardExpiration { get; set; }
    }
}
