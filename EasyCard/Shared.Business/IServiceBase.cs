using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business
{
    public interface IServiceBase
    {
        Task<OperationResponse> SaveChanges(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null);
    }
}
