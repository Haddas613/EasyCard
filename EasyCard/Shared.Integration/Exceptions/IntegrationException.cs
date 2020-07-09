using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Exceptions
{
    /// <summary>
    /// This exception type should contain message appropriate to user
    /// </summary>
    public class IntegrationException : Exception
    {
        public string MessageId { get; }

        public IntegrationException(string message, string messageId)
            : base(message)
        {
            this.MessageId = messageId;
        }
    }
}
