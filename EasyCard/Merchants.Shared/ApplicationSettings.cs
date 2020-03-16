using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared
{
    public class ApplicationSettings
    {
        public string DefaultStorageConnectionString { get; set; }

        public string RequestsLogStorageTable { get; set; }
    }
}
