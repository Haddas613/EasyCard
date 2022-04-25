using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.External.ThreeDS
{
    public class Versioning3DsResponse
    {
        public string ThreeDSMethodUrl { get; set; }

        public string ThreeDSMethodData { get; set; }

        public string ErrorMessage { get; set; }

        public bool PassThrough { get; set; }

        public string ThreeDSServerTransID { get; set; }
    }
}
