using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared
{
    public class ApplicationSettings
    {
        public string DefaultStorageConnectionString { get; set; }

        public string PublicStorageConnectionString { get; set; }

        public string ClearingHouseRequestsLogStorageTable { get; set; }

        public string UpayRequestsLogStorageTable { get; set; }

        public string EasyInvoiceRequestsLogStorageTable { get; set; }

        public string RequestsLogStorageTable { get; set; }

        public string EncrKeyForSharedApiKey { get; set; }

        public string ShvaRequestsLogStorageTable { get; set; }

        public string NayaxRequestsLogStorageTable { get; set; }

        public string PublicBlobStorageTable { get; set; }
    }
}
