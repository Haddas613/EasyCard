using System;
using System.Collections.Generic;
using System.Text;

namespace Upay.Models
{
    public class MsgModel
    {
        public HeaderBase header { get; set; }
        public RequestModel request { get; set; }
    }
}
