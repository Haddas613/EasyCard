﻿using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.External.ThreeDS
{
    public class Versioning3DsRequest
    {
        [JsonConverter(typeof(TrimmingJsonConverter), true, true)]
        [Required]
        public string CardNumber { get; set; }

        public Guid? TerminalID { get; set; }
    }
}
