using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class ECInvoiceGenerateCertificateRequest
    {
        [JsonProperty("firstLastName")]
        public string FirstLastName { get; set; }
    }
}
