﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ConsumerSummary
    {
        public Guid ConsumerID { get; set; }

        public Guid TerminalID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        public string ConsumerName { get; set; }

        public string ConsumerPhone { get; set; }
    }
}