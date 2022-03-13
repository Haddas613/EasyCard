using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    public class BitRefundResponse : BitBaseResponse
    {
        public bool Success { get; set; } = true;

        public string RequestStatusCode { get; set; }

        public string RequestStatusDescription { get; set; }
    }
}
