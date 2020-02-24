using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public string EntityReference { get; }

        public string EntityType { get; }

        public EntityNotFoundException(string message, string entityType, string entityReference) : base(message)
        {
            this.EntityType = entityType;

            this.EntityReference = entityReference;
        }
    }
}
