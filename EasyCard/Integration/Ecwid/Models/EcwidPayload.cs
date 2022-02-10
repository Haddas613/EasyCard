using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    public class EcwidPayload
    {
        public string StoreId { get; set; }

        public string ReturnUrl { get; set; }

        public EcwidMerchantSettings MerchantAppSettings { get; set; }

        public EcwidCart Cart { get; set; }

        public string Token { get; set; }

        public string Lang { get; set; }
    }
}
