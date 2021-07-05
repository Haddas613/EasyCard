using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class NayaxUpdateTranRecordRequest
    {
        public string TranRecord { get; set; }

        public string Vuid { get; set; }

        public string Uid { get; set; }
    }
}
