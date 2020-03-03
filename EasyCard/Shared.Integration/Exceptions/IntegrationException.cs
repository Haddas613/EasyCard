using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Exceptions
{
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
