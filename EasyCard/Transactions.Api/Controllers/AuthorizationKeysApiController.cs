using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using TransactionsApi.Models;

namespace TransactionsApi.Controllers
{
    [Route("api/authorizationKeys")]
    [ApiController]
    public class AuthorizationKeysApiController : ApiControllerBase
    {
        [HttpPost]
        [Route("reset")]
        public async Task<ActionResult<AuthorizationKeys>> ResetKeys()
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