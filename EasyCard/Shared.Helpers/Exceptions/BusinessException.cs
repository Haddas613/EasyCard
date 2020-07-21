using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public class BusinessException : Exception
    {
        public IEnumerable<Error> Errors { get; set; }

        public BusinessException(string message, IEnumerable<Error> errors = null)
            : base(message)
        {
            Errors = errors;
        }
    }
}
