using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.External.ThreeDS
{
    public class Authenticate3DsCallback
    {
        public string ThreeDSServerTransID { get; set; }

        public string TransStatus { get; set; }

        public string AuthenticationValue { get; set; }

        public string Eci { get; set; }

        public string Xid { get; set; }
    }
}
