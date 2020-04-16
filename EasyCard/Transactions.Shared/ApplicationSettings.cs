using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared
{
    public class ApplicationSettings
    {
        public string DefaultStorageConnectionString { get; set; }

        public string ShvaRequestsLogStorageTable { get; set; }

        public string ClearingHouseRequestsLogStorageTable { get; set; }

        public int FiltersGlobalPageSizeLimit { get; set; }
    }
}
