﻿using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class TransactionsFilter : FilterBase
    {
        public long TerminalID { get; set; }
    }
}
