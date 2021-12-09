using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class NayaxRequest
    {
        /// <summary>
        /// SHVA Terminal Number
        /// </summary>
        public string ProcessorTerminal { get; set; }

        /// <summary>
        /// Aditek Identifier for device  
        /// </summary>
        public string ClientToken { get; set; }
    }
}
