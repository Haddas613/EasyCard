using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    public class LoginResponse
    {
        /// <summary>
        /// Gets or Sets Success status
        /// </summary>
        [DataMember(Name = "success")]
        public string Success { get; set; }

        /// <summary>
        /// Gets or Sets ErrorMessage
        /// </summary>
        [DataMember(Name = "errormessage")]
        public string ErrorMessage { get; set; }
    }
}
