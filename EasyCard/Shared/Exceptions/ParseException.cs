using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Exceptions
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
            
        }
    }
}
