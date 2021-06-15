using System;
using System.Collections.Generic;
using System.Text;

namespace Upay.Models
{
    public class RequestModel
    {
        public string mainaction { get; set; }
        public string minoraction { get; set; }
        public string encoding { get; set; }
        public int numbertemplate { get; set; }
        public ParameterBase parameters { get; set; }
    }
}
