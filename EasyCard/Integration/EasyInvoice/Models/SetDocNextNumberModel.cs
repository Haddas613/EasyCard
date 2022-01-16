using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    [Serializable]
    public class SetDocNextNumberModel
    {
        [JsonProperty("documentType")]
        public string DocumentType { get; set; }

        [JsonProperty("nextDocumentNumber")]
        public long NextDocumentNumber { get; set; }
    }
}
