using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reporting.Business.Services;
using Reporting.Shared.Models;
using Reporting.Shared.Models.Admin;
using Shared.Api;
using Shared.Helpers.Services;

namespace Reporting.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/admin")]
    //[Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
    [ApiController]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class AdminController : ApiControllerBase
    {
        private readonly IAdminService adminService;

        public AdminController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        [HttpGet]
        [Route("getSmsTimelines")]
        public async Task<ActionResult<AdminSmsTimelines>> GetSmsTotals([FromQuery]DashboardQuery query)
        {
            var res = await adminService.GetSmsTotals(query);

            return Ok(res);
        }
    }
}
