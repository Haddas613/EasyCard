using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api
{
    public class Policy
    {
        public const string Terminal = "terminal_rest_api";
        public const string TerminalOrMerchantFrontendOrAdmin = "terminal_or_merchant_frontend_or_admin";
        public const string MerchantFrontend = "merchant_frontend";
        public const string AnyAdmin = "admin";
    }
}
