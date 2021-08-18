using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.UpdateParameters
{
    public class SendTerminalsToQueueResponse : OperationResponse
    {
        public int Count { get; set; }
    }
}
