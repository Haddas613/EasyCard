using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Business.Security
{
    public static class HttpContextAccessorWrapperExtension
    {
        public static void ApplyAuditInfo(this IAuditEntity auditEntity, IHttpContextAccessorWrapper httpContextAccessor)
        {
            var user = httpContextAccessor.GetUser();

            auditEntity.OperationDoneBy = user?.GetDoneBy();
            auditEntity.OperationDoneByID = user?.GetDoneByID();
            auditEntity.SourceIP = httpContextAccessor.GetIP();
            auditEntity.CorrelationId = httpContextAccessor.GetCorrelationId();
        }
    }
}
