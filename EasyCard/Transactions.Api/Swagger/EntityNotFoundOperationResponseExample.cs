using Shared.Api.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedApi = Shared.Api;

namespace Transactions.Api.Swagger
{
    public class EntityNotFoundOperationResponseExample : IExamplesProvider<OperationResponse>
    {
        public OperationResponse GetExamples()
        {
            return new OperationResponse
            {
                Status = SharedApi.Models.Enums.StatusEnum.Error,
                CorrelationId = Guid.NewGuid().ToString(),
                Message = "Entity Not Found",
                EntityType = "Consumer"
            };
        }
    }
}
