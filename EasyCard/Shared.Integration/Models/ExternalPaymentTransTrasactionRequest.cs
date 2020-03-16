using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ExternalPaymentTransTrasactionRequest
    {
        public Object ProcessorSettings { get; set; }

        public string DATAToTrans { get; set; }


    }
}
