using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    internal class EcwidRequest
    {
        public int StoreID { get; set; }

        public string Lang { get; set; }

        public string ReturnURL { get; set; }

        public JToken MerchantApiSettings { get; set; }

        public EcwidCart Cart { get; set; }

        public string Token { get; set; }
    }
}
