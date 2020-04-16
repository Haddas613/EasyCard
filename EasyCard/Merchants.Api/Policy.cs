using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api
{
    public class Policy
    {
        public const string AnyAdmin = "any_admin";
        public const string OnlyBillingAdmin = "only_billing_admin";
        public const string ManagementApi = "management_api";
        public const string ManagementApiOrBillingAdmin = "management_api_or_billing_admin";
    }
}
