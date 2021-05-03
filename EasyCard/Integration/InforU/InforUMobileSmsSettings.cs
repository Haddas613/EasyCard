using System;
using System.Collections.Generic;
using System.Text;

namespace InforU
{
    public class InforUMobileSmsSettings
    {
        public string InforUMobileSmsRequestsLogStorageTable { get; set; } = "sms";

        public string InforUMobileBaseUrl { get; set; } = "https://uapi.inforu.co.il/";

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
