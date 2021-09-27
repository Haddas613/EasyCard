using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models
{
    public class ApplicationSettings
    {
        public string EncrKeyForSharedApiKey { get; set; }

        public string AzureSignalRConnectionString { get; set; }
    }
}
