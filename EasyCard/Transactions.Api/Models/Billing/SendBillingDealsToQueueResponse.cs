﻿using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Billing
{
    public class SendBillingDealsToQueueResponse : OperationResponse
    {
        public int Count { get; set; }
    }
}
