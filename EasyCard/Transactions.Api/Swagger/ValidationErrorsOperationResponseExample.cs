using Shared.Api.Models;
using Shared.Helpers;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedApi = Shared.Api;

namespace Transactions.Api.Swagger
{
    public class ValidationErrorsOperationResponseExample : IExamplesProvider<OperationResponse>
    {
        public OperationResponse GetExamples()
        {
            return new OperationResponse
            {
                Status = SharedApi.Models.Enums.StatusEnum.Error,
                CorrelationId = Guid.NewGuid().ToString(),
                Message = "Validation Errors",
                Errors = new List<Error> { new Error { Code = "paymentRequestAmount", Description = "Could not convert string to decimal: 1200$. Path 'paymentRequestAmount', line 10, position 33." } }
            };
        }
    }
}
