﻿using Newtonsoft.Json.Linq;
using Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.Terminal
{
    public class ExternalSystemDetails
    {
        public long ExternalSystemDetailsID { get; set; }

        public ExternalSystemSummary ExternalSystem { get; set; }

        // TODO: move inside ExternalSystemSummary (?)
        public ExternalSystemTypeEnum ExternalSystemType { get; set; }

        /// <summary>
        /// SHVA or other system terminal ID
        /// </summary>
        public string ExternalProcessorReference { get; set; }

        //public JObject Configuration { get; set; }

        //

    }
}
