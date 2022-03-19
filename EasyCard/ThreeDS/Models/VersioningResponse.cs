using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class VersioningResponse
    {
        /// <summary>
        /// like UID in proper SHVA transaction
        /// </summary>
        public string ThreeDSServerTransID { get; set; }

        public string AcsStartProtocolVersion { get; set; }

        public string AcsEndProtocolVersion { get; set; }

        public string DsStartProtocolVersion { get; set; }

        public string DsEndProtocolVersion { get; set; }

        public string HighestCommonSupportedProtocolVersion { get; set; }

        public string AcsInfoInd { get; set; }

        public string ThreeDSMethodURL { get; set; }

        public ThreeDSMethodDataForm ThreeDSMethodDataForm { get; set; }

        public ThreeDSMethodData ThreeDSMethodData { get; set; }
    }
}
