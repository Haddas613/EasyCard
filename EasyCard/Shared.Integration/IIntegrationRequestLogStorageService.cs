using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Integration
{
    public interface IIntegrationRequestLogStorageService
    {
        Task Save(IntegrationMessage entity);

        Task<IntegrationMessage> Get(DateTime requestDate, string correlationId);

        string StorageTableName { get; }
    }
}
