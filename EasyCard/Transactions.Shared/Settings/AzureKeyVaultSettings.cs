using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Settings
{
    public class AzureKeyVaultSettings
    {
        public string KeyVaultUrl { get; set; }
        public string AzureADApplicationId { get; set; }
        public string AzureADApplicationSecret { get; set; }
        public string AzureADApplicationTenant { get; set; }
    }
}
