using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api
{
    public class RequestResponseLoggingSettings
    {
        public string StorageConnectionString { get; set; }

        public string RequestsLogStorageTable { get; set; }
    }
}
