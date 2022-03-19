using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class ChallengeRequest
    {
        public string MessageType { get; set; }
        public string ThreeDSServerTransID { get; set; }
        public string AcsTransID { get; set; }
        public string ChallengeWindowSize { get; set; }
        public string MessageVersion { get; set; }
    }
}
