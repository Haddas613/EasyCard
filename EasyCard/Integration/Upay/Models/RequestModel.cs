using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    public class RequestModel
    {
        /// <summary>
        /// Gets or Sets  Mainaction
        /// </summary>
        [DataMember(Name = "mainaction")]
        public string Mainaction { get; set; }

        /// <summary>
        /// Gets or Sets  Minoraction
        /// </summary>
        [DataMember(Name = "minoraction")]
        public string Minoraction { get; set; }

        /// <summary>
        /// Gets or Sets  Encoding
        /// </summary>
        [DataMember(Name = "encoding")]
        public string Encoding { get; set; }

        /// <summary>
        /// Gets or Sets  Numbertemplate
        /// </summary>
        [DataMember(Name = "numbertemplate")]
        public int Numbertemplate { get; set; }

        /// <summary>
        /// Gets or Sets  Parameters
        /// </summary>
        [DataMember(Name = "parameters")]
        public ParameterBase Parameters { get; set; }
    }
}
