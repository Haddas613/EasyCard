using AutoMapper;
using Merchants.Api.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using System;
using System.Threading.Tasks;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/system")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class SystemLogApiController : ApiControllerBase
    {
        private readonly IMapper mapper;

        public SystemLogApiController(IMapper mapper)
        {
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<UserResponse>> GetUsers([FromQuery] GetUsersFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}
