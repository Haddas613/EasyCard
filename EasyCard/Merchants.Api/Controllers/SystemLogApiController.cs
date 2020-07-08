using AutoMapper;
using Merchants.Api.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Logging;
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
        private readonly IDatabaseLogService databaseLogService;

        public SystemLogApiController(IMapper mapper, IDatabaseLogService databaseLogService)
        {
            this.mapper = mapper;
            this.databaseLogService = databaseLogService;
        }

        [HttpGet]
        public async Task<ActionResult<DatabaseLogEntry>> GetLogEntries([FromQuery] GetUsersFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}
