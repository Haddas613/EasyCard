﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.User
{
    public class InviteUserRequest
    {
        public long MerchantID { get; set; }

        public string InviteMessage { get; set; }
    }
}
