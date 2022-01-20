using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckoutPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Models;

namespace CheckoutPortal.Controllers
{
    [Route("api/3ds-version")]
    [ApiController]
    public class ThreeDSController : ControllerBase
    {
        [HttpPost]
        public Task<ActionResult<OperationResponse>> VersionResponse(ThreeDSecureGetVersionReq request)
        {
            return null;
        }
    }
}