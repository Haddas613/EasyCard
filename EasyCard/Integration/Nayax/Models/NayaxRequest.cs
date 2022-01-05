using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nayax.Models
{
    public class NayaxRequest
    {
        /// <summary>
        /// SHVA Terminal Number
        /// </summary>
        [Required]
        public string ProcessorTerminal { get; set; }

        /// <summary>
        /// Aditek Identifier for device
        /// </summary>
        [Required]
        public string ClientToken { get; set; }
    }
}
