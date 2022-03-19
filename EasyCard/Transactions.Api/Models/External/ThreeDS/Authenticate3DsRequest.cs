﻿using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.External.ThreeDS
{
    public class Authenticate3DsRequest
    {
        public Guid TerminalID { get; set; }

        public string ThreeDSServerTransID { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter), true, true)]
        public string CardNumber { get; set; }
    }
}
