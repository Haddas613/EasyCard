using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Merchants.Api.Models.Integrations.EasyInvoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/easy-invoice")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class EasyInvoiceApiController : ApiControllerBase
    {
        [HttpPost]
        [Route("create-customer")]
        public async Task<ActionResult<OperationResponse>> CreateCustomer(CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new OperationResponse(EasyInvoiceMessagesResource.CustomerCreatedSuccessfully, StatusEnum.Success));
        }
    }
}