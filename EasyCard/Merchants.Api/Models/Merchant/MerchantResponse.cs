using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    public class MerchantResponse : MerchantBase
    {
        public IEnumerable<TerminalSummary> Terminals { get; set; }

        public IEnumerable<UserSummary> Users { get; set; }
    }
}
