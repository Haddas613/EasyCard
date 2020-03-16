using System;
using System.Runtime.Serialization;
using System.Text;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Business error details
    /// </summary>
    [DataContract]
    public partial class Error
    {
        /// <summary>
        /// Gets or Sets Code
        /// </summary>
        [DataMember(Name = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}
