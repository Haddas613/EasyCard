using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Configuration
{
    public class ThreedDSGlobalConfiguration
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string PspID { get; set; } //Distributer ID 

        public string BaseUrl { get; set; } =  "https://3dsc.shva.co.il/CreditCartPay/"; //"https://3dsc.shva.co.il/3DSInterface/";//"https://3dsc.shva.co.il/CreditCartPay";//  //"https://3dsc-qa.shva.co.il/CreditCartPay/"; // please keep end slash

        public string CertificateThumbprint { get; set; }

        public string MerchantURL { get; set; } = "https://ecng-testwebstore.azurewebsites.net/Home/Notification3Ds";//"https://api.e-c.co.il/api/external/3ds/authenticateCallback"; //;
    }
}
