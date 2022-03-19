using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Contract
{
    public class Authenticate3DsRequestModel
    {
        public string MerchantNumber { get; set; }

        public string ThreeDSServerTransID { get; set; }

        public string CardNumber { get; set; }
    }
}
