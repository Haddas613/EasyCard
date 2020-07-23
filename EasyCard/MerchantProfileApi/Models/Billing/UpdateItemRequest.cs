﻿using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class UpdateItemRequest
    {
        public Guid ItemID { get; set; }

        public string ItemName { get; set; }

        public decimal Price { get; set; }

        public CurrencyEnum Currency { get; set; }

        public byte[] UpdateTimestamp { get; set; }
    }
}