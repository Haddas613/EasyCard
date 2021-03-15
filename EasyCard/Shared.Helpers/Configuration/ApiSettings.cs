using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Configuration
{
    public class ApiSettings
    {
        public string Version { get; set; }

        /// <summary>
        /// Merchant's page
        /// </summary>
        public string MerchantProfileURL { get; set; }

        /// <summary>
        /// Backoffice portal and merchants management api
        /// </summary>
        public string MerchantsManagementApiAddress { get; set; }

        public string TransactionsApiAddress { get; set; }

        public string ReportingApiAddress { get; set; }

        public string CheckoutPortalUrl { get; set; }
    }
}
