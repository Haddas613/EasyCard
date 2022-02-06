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
        public string threeDSServerTransID { get; set; }
        public string acsStartProtocolVersion { get; set; }
        public string acsEndProtocolVersion { get; set; }

        public string dsStartProtocolVersion { get; set; }

        public string dsEndProtocolVersion { get; set; }

        public string highestCommonSupportedProtocolVersion { get; set; }

        public string acsInfoInd { get; set; }

        public string threeDSMethodURL { get; set; }
        public ThreeDSMethodDataForm threeDSMethodDataForm { get; set; }
        public ThreeDSMethodData threeDSMethodData { get; set; }
    }
}
