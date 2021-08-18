using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class ObjectInAuthRequestParams
    {
        public string otp { get; set; }
        public ObjectInAuthRequestParams(string otp)
        {
            this.otp = otp;
        }
    }
}
