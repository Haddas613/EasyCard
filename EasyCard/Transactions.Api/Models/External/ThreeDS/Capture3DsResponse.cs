using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.External.ThreeDS
{
    public class Capture3DsResponse
    {
        public string MessageType { get; set; }

        public string ThreeDSServerTransID { get; set; }

        public string AcsTransID { get; set; }

        public string ChallengeCompletionInd { get; set; }

        public string MessageVersion { get; set; }

        public string TransStatus { get; set; }

        public bool Success
        {
            get { return TransStatus == "Y"; }
        }
    }
}
