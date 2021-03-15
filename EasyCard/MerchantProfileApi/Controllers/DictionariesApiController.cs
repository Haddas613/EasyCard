using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServerClient;
using MerchantProfileApi.Extensions;
using MerchantProfileApi.Models.Dictionaries;
using MerchantProfileApi.Models.Terminal;
using MerchantProfileApi.Services;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileApi;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers.Security;

namespace MerchantProfileApi.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/dictionaries")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DictionariesApiController : ApiControllerBase
    {
        [HttpGet]
        [Route("merchant")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<MerchantDictionaries>> GetMerchantDictionaries()
        {
            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture?.Culture;

            var dictionaries = DictionariesService.GetDictionaries(culture);
            return Ok(dictionaries);
        }
    }
}