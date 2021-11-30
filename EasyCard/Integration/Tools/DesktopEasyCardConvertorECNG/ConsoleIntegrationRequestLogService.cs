using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Integration
{
    public class ConsoleIntegrationRequestLogService : IIntegrationRequestLogStorageService
    {
        private readonly ILogger logger;

        public ConsoleIntegrationRequestLogService(
            ILogger<ConsoleIntegrationRequestLogService> logger)
        {
            this.logger = logger;
        }

        public string StorageTableName => throw new NotImplementedException();

        public Task<IntegrationMessage> Get(DateTime requestDate, string correlationId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IntegrationMessage>> GetAll(string entityID)
        {
            throw new NotImplementedException();
        }

        public async Task Save(IntegrationMessage entity)
        {
            logger.LogInformation(entity.Request); // TODO
        }
    }
}
