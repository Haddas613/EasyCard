﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions.Enums
{
    public enum TranTypeEnum
    {
        charge = 01,
        //forced_transaction = 03,
        //cashback = 06,
        //cash = 07,
        refund = 53,
        //loading = 55
    }
}