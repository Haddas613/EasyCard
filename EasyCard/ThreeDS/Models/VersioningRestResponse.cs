using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class VersioningRestResponse
    {
        public ErrorVersion errorDetails { get; set; }
        public VersioningRequest versioningRequest { get; set; }
        public VersioningResponse versioningResponse { get; set; }
    }
}
