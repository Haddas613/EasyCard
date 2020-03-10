using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Api
{
    public interface IRequestLogStorageService
    {
        Task Save(LogRequestEntity entity);

        Task<LogRequestEntity> Get(DateTime requestDate, string correlationId);
    }
}
