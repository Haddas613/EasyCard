using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    /// <summary>
    /// Bit api does not return error status codes on a number of errors.
    /// Instead MessageException field should be checked
    /// </summary>
    public class BitBaseResponse
    {
        public string MessageException { get; set; }

        public string MessageCode { get; set; }
    }
}
