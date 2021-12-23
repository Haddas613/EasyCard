using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    [Serializable]
    public class GetDocNumberModel
    {
        [JsonProperty("documentType")]
        public string DocumentType { get; set; }
    }
}
