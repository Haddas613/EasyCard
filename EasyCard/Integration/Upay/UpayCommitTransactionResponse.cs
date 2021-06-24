using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Upay
{
    class UpayCommitTransactionResponse : AggregatorCommitTransactionResponse
    {
            public bool Success { get; set; }
    }
}
