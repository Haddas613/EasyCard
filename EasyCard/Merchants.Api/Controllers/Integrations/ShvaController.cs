using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Merchants.Api.Models.Integrations.Shva;
using Merchants.Api.Models.Terminal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/shva")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class ShvaController : ApiControllerBase
    {
        [HttpPost]
        [Route("new-password")]
        public async Task<ActionResult<OperationResponse>> NewPassword(NewPasswordRequest request)
        {
            if (request.TerminalID.HasValue)
            {
                //TODO process terminal
            }
            else if (request.TerminalTemplateID.HasValue)
            {
                //TODO process terminal template
            }
            else
            {
                return BadRequest(ShvaMessages.EitherTerminalOrTerminalTemplateIDMustBeSpecified);
            }

            return Ok(new OperationResponse(ShvaMessages.NewPasswordSetSuccessfully, StatusEnum.Success));
        }

        [HttpPost]
        [Route("test-connection")]
        public async Task<ActionResult<OperationResponse>> TestConnection(ExternalSystemRequest request)
        {
            return Ok(new OperationResponse(ShvaMessages.ConnectionSuccess, StatusEnum.Success));
        }
    }
}