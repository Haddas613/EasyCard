﻿using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class TransmissionFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }
    }
}