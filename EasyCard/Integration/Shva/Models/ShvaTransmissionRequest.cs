using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    public class ShvaTransmissionRequest
    {
        // TODO: what should be in this field
        public object ProcessorSettings { get; set; }

        // TODO: we need to use appropriate oject, for example array of strings
        public string DATAToTrans { get; set; }
    }
}
