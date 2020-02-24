using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Exceptions
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
            
        }
    }
}
