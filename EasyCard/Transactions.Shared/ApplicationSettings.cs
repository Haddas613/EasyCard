using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared
{
    public class ApplicationSettings
    {
        public string DefaultStorageConnectionString { get; set; }

        public string ShvaRequestsLogStorageTable { get; set; }

        public string NayaxRequestsLogStorageTable { get; set; }

        public string ClearingHouseRequestsLogStorageTable { get; set; }

        public string EasyInvoiceRequestsLogStorageTable { get; set; }

        public int FiltersGlobalPageSizeLimit { get; set; } = 1000;

        public int TransmissionMaxBatchSize { get; set; } = 10;

        public int BillingDealsMaxBatchSize { get; set; } = 10;

        public string RequestsLogStorageTable { get; set; }

        public string EncrKeyForSharedApiKey { get; set; }

        public string EmailTableName { get; set; }

        public string EmailQueueName { get; set; }

        public string InvoiceQueueName { get; set; }
    }
}
