using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Integration
{
    public interface IIntegrationRequestLogStorageService
    {
        Task Save(IntegrationMessage entity);

        string StorageTableName { get; }

        Task<IEnumerable<IntegrationMessage>> GetAll(string entityID);
    }
}
