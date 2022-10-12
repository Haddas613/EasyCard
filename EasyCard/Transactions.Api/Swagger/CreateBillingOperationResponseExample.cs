using Swashbuckle.AspNetCore.Filters;
using Shared.Api.Models;
using System;
using SharedApi = Shared.Api;

namespace Transactions.Api.Swagger
{
    public class CreateBillingOperationResponseExample : IExamplesProvider<OperationResponse>
    {
        public OperationResponse GetExamples()
        {
            return new OperationResponse
            {
                Status = SharedApi.Models.Enums.StatusEnum.Success,
                EntityUID = Guid.NewGuid(),
                Message = "Billing Deal Created"
            };
        }
    }
}
