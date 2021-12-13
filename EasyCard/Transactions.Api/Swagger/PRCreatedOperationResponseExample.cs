using Newtonsoft.Json.Linq;
using Shared.Api.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedApi = Shared.Api;

namespace Transactions.Api.Swagger
{
    public class PRCreatedOperationResponseExample : IExamplesProvider<OperationResponse>
    {
        public OperationResponse GetExamples()
        {
            return new OperationResponse
            {
                Status = SharedApi.Models.Enums.StatusEnum.Success,
                EntityUID = Guid.NewGuid(),
                Message = "Payment Request Created",
                AdditionalData = JObject.FromObject(new
                {
                    Url = "https://checkout.e-c.co.il/i?r=4xgxjJrpKhOgdJa01YhT9fMy7AHdUi1XmWralF9lbbnB0nTGB1vw%3d%3d"
                })
            };
        }
    }
}
