﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Configuration
{
    public class ThreedDSGlobalConfiguration
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string PspID { get; set; } //Distributer ID 

        public string BaseUrl { get; set; } = "https://3dsc-qa.shva.co.il/CreditCartPay"; // please do not end with slash

        public string CertificateThumbprint { get; set; }

        public string MerchantURL { get; set; } = "https://ecng-testwebstore.azurewebsites.net/Home/Notification3Ds";
    }
}
