using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    public class EcwidTransactionExtension
    {
        public string ReferenceTransactionID { get; set; }

        public string StoreID { get; set; }

        public string Token { get; set; }

        public bool Valid()
        {
            return ReferenceTransactionID != null && StoreID != null && Token != null;
        }
    }
}
