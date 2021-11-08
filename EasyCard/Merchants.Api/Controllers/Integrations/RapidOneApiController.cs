using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Merchants.Api.Models.Integrations.RapidOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RapidOne;
using Shared.Api;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/rapidone")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RapidOneApiController : ApiControllerBase
    {
        private readonly RapidOneInvoicing rapidOneInvoicing;

        public RapidOneApiController(RapidOneInvoicing rapidOneInvoicing)
        {
            this.rapidOneInvoicing = rapidOneInvoicing;
        }

        [HttpGet]
        [Route("companies")]
        public async Task<ActionResult<IEnumerable<CompanySummary>>> GetCompanies(string baseurl, string token)
        {
            var result = await rapidOneInvoicing.GetCompanies(baseurl, token);

            var response = result.Select(r => new CompanySummary { DbName = r.DbName, Name = r.Name });

            return Ok(response);
        }

        [HttpGet]
        [Route("branches")]
        public async Task<ActionResult<IEnumerable<BranchSummary>>> GetBranches(string baseurl, string token)
        {
            var result = await rapidOneInvoicing.GetBranches(baseurl, token);

            var response = result.Where(r => r.Active)
                .Select(r => new BranchSummary { BranchID = r.BranchID, Name = r.Name });

            return Ok(response);
        }

        [HttpGet]
        [Route("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentSummary>>> GetDepartments(string baseurl, string token, int branchId)
        {
            var result = await rapidOneInvoicing.GetDepartments(baseurl, token, branchId);

            var response = result.Where(r => r.Active)
                .Select(r => new DepartmentSummary { BranchID = r.BranchID, DepartmentID = r.DepartmentID, Name = r.Name, Active = r.Active });

            return Ok(response);
        }
    }
}