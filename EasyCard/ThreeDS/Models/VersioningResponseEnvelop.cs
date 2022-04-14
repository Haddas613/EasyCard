using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class VersioningResponseEnvelop
    {
        public VersioningErrorDetails ErrorDetails { get; set; }

        public VersioningResponse VersioningResponse { get; set; }
    }
}
