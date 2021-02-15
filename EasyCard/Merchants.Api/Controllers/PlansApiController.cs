using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Services;
using Merchants.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Models;
using Z.EntityFramework.Plus;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/plans")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)] // TODO: bearer
    [ApiController]
    public class PlansApiController : ApiControllerBase
    {
        private readonly IPlansService plansService;
        private readonly IFeaturesService featuresService;
        private readonly ITerminalTemplatesService templatesService;
        private readonly IMapper mapper;

        public PlansApiController(IPlansService plansService, IFeaturesService featuresService, IMapper mapper, ITerminalTemplatesService templatesService)
        {
            this.plansService = plansService;
            this.featuresService = featuresService;
            this.mapper = mapper;
            this.templatesService = templatesService;
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<PlanSummary>>> GetPlans()
        {
            var plansQueryFuture = plansService.GetQuery().Include(p => p.TerminalTemplate).Future();
            var featuresQueryFuture = featuresService.GetQuery().Future();

            var dbPlans = await plansQueryFuture.ToListAsync();
            var features = (await featuresQueryFuture.ToListAsync()).ToDictionary(k => k.FeatureID, v => mapper.Map<FeatureSummary>(v));

            var plans = new List<PlanSummary>(dbPlans.Count);

            foreach (var plan in dbPlans)
            {
                var responsePlan = mapper.Map<PlanSummary>(plan);
                responsePlan.Features = new List<FeatureSummary>();

                if (plan.TerminalTemplate?.EnabledFeatures.Count > 0)
                {
                    foreach (var featureID in plan.TerminalTemplate?.EnabledFeatures)
                    {
                        if (features.ContainsKey(featureID))
                        {
                            responsePlan.Features.Add(features[featureID]);
                        }
                    }
                }

                plans.Add(responsePlan);
            }

            var response = new SummariesResponse<PlanSummary>
            {
                NumberOfRecords = plans.Count,
                Data = plans
            };

            return response;
        }
    }
}