using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Logging
{
    public class ApiErrorLogFormatter
    {
        public static string ExceptionFormatWithDetails(Exception ex, string correlationId)
        {
            var details = GetDetailsForException(ex);

            return $"{ex.GetType().Name}; Message: {ex.Message};{(details != null ? $" Details: {details}" : string.Empty)}";
        }

        private static string GetDetailsForException(Exception ex)
        {
            return ex switch
            {
                EntityNotFoundException enfEx => $"{nameof(enfEx.EntityType)} : {enfEx.EntityType}, {nameof(enfEx.EntityReference)} : {enfEx.EntityReference}",
                EntityConflictException ecnEx => $"{nameof(ecnEx.EntityType)} : {ecnEx.EntityType}, {nameof(ecnEx.ConflictDetails)} : {ecnEx.ConflictDetails}",
                _ => null
            };
        }
    }
}