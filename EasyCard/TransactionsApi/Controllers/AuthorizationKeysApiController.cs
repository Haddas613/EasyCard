using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using TransactionsApi.Models;

namespace TransactionsApi.Controllers
{
    [Route("api/authorizationKeys")]
    [ApiController]
    public class AuthorizationKeysApiController : ControllerBase
    {
        [HttpPost]
        [Route("reset")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorizationKeys))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        public async Task<IActionResult> ResetKeys()
        {
            // TODO: implementation
            var keys = new AuthorizationKeys
            {
                PrivateKey = Guid.NewGuid().ToString(),
                PublicKey = Guid.NewGuid().ToString()
            };

            return new JsonResult(keys);
        }
    }
}