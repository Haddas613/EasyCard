using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProfileApi
{
    public class Policy
    {
        /// <summary>
        /// Noninteractive api access
        /// </summary>
        public const string Terminal = "terminal_rest_api";

        /// <summary>
        /// Noninteractive api access or merchant UI
        /// </summary>
        public const string TerminalOrMerchantFrontend = "terminal_or_merchant_frontend";

        /// <summary>
        /// Merchant UI access
        /// </summary>
        public const string MerchantFrontend = "merchant_frontend";
    }
}
