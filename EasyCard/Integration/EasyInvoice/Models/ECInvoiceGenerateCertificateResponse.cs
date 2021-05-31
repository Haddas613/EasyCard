using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class ECInvoiceGenerateCertificateResponse
    {
        [JsonProperty("keyStorePassword")]
        public string KeyStorePassword { get; set; }
    }
}
