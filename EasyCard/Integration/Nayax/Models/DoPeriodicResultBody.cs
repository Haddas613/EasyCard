using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class DoPeriodicResultBody
    {
        public int statusCode { get; set; }
        public string statusMessage { get; set; }
        public string report { get; set; }
        public string ackNumber { get; set; }
    }
}
