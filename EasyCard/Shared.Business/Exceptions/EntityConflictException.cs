using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Business.Exceptions
{
    public class EntityConflictException : Exception
    {
        public string ConflictDetails { get; }

        public string EntityType { get; }

        public EntityConflictException(string message, string entityType, string conflictDetails = null)
            : base(message)
        {
            EntityType = entityType;

            ConflictDetails = conflictDetails;
        }
    }
}
