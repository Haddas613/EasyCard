using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Events
{
    public class CustomEvent : CustomEventBase
    {
        [JsonIgnore]
        public object Entity { get; set; }
    }
}
