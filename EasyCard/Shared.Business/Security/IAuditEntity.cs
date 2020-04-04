using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Business.Security
{
    public interface IAuditEntity
    {
        string OperationDoneBy { get; set; }

        Guid? OperationDoneByID { get; set; }

        string CorrelationId { get; set; }

        string SourceIP { get; set; }
    }
}
