using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.External.ThreeDS
{
    public class Authenticate3DsResponse
    {
        public string ErrorMessage { get; set; }

        public string ErrorDetail { get; set; }

        public string AcsURL { get; set; }

        public string Base64EncodedChallengeRequest { get; set; }

        public string ThreeDSServerTransID { get; set; }
    }
}
