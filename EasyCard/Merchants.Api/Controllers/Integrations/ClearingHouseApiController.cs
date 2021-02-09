using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Merchants.Api.Models.Integrations.ClearingHouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/clearing-house")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class ClearingHouseApiController : ApiControllerBase
    {
        [HttpGet]
        [Route("get-customer/{customerID}")]
        public async Task<ActionResult<ClearingHouseCustomerResponse>> CreateCustomer(string customerID)
        {
            var response = new ClearingHouseCustomerResponse
            {
                UserName = "(Example) Not an actual user",
                Email = "example@clearinghouse.com",
                MerchantReference = "123ABC456EDE",
                ShvaTerminalReference = "SHVA1235455"
            };

            return Ok(response);
        }
    }
}