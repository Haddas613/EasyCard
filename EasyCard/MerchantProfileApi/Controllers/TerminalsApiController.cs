using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BasicServices.BlobStorage;
using IdentityServerClient;
using MerchantProfileApi.Extensions;
using MerchantProfileApi.Models.Terminal;
using Merchants.Business.Extensions;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ProfileApi;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers.Security;
using Z.EntityFramework.Plus;

namespace MerchantProfileApi.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/terminals")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TerminalsApiController : ApiControllerBase
    {
        private readonly ITerminalsService terminalsService;
        private readonly IMerchantsService merchantsService;
        private readonly IMapper mapper;
        private readonly IUserManagementClient userManagementClient;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly ICryptoServiceCompact cryptoServiceCompact;
        private readonly IFeaturesService featuresService;
        private readonly IBlobStorageService blobStorageService;

        public TerminalsApiController(
            IMerchantsService merchantsService,
            ITerminalsService terminalsService,
            IMapper mapper,
            IUserManagementClient userManagementClient,
            ISystemSettingsService systemSettingsService,
            ICryptoServiceCompact cryptoServiceCompact,
            IFeaturesService featuresService,
            IBlobStorageService blobStorageService)
        {
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
            this.mapper = mapper;
            this.userManagementClient = userManagementClient;
            this.systemSettingsService = systemSettingsService;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.featuresService = featuresService;
            this.blobStorageService = blobStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TerminalSummary>>> GetTerminals([FromQuery] TerminalsFilter filter)
        {
            var merchantId = User.GetMerchantID();

            var query = terminalsService.GetTerminals().Filter(filter);

            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            var response = new SummariesResponse<TerminalSummary>();

            response.Data = await mapper.ProjectTo<TerminalSummary>(query.ApplyPagination(filter)).Future().ToListAsync();
            response.NumberOfRecords = numberOfRecordsFuture.Value;

            return Ok(response);
        }

        [HttpGet]
        [Route("{terminalID}")]
        public async Task<ActionResult<TerminalResponse>> GetTerminal([FromRoute]Guid terminalID)
        {
            //not terminalsService.GetTerminal so it can still be accessed as read only even if it's disabled
            var terminal = mapper.Map<TerminalResponse>(EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID)));

            var merchant = EnsureExists(await merchantsService.GetMerchants().FirstOrDefaultAsync(m => m.MerchantID == terminal.MerchantID));

            terminal.MerchantName = merchant.BusinessName;

            var systemSettings = await systemSettingsService.GetSystemSettings();

            mapper.Map(systemSettings, terminal);

            return Ok(terminal);
        }

        // TODO: concurrency check, handle exceptions
        [HttpPut]
        [Route("{terminalID}")]
        public async Task<ActionResult<OperationResponse>> UpdateTerminal([FromRoute]Guid terminalID, [FromBody]UpdateTerminalRequest model)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            mapper.Map(model, terminal);

            await terminalsService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.TerminalUpdated, StatusEnum.Success, terminalID));
        }

        [HttpPost]
        [Route("{terminalID}/merchantlogo")]
        [Consumes("multipart/form-data")]
        //[RequestSizeLimit(1000000)]
        public async Task<ActionResult<OperationResponse>> UploadMerchantLogo([FromRoute]Guid terminalID, [FromForm]IFormFile file)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            if (file == null || file.Length <= 0)
            {
                return BadRequest(new OperationResponse { Message = Messages.FileRequired, Status = StatusEnum.Error });
            }

            if (file.Length > 1000000)
            {
                return BadRequest(new OperationResponse { Message = Messages.MaxFileSizeIs1MB, Status = StatusEnum.Error });
            }

            //TODO: more strict check?
            if (!file.ContentType.Contains("image"))
            {
                return BadRequest(new OperationResponse { Message = Messages.OnlyImagesAreAllowed, Status = StatusEnum.Error });
            }

            var response = new OperationResponse { Message = Messages.TerminalUpdated, Status = StatusEnum.Success };

            using (var uploadStream = file.OpenReadStream())
            {
                uploadStream.Seek(0, SeekOrigin.Begin);

                var filename = $"merchantdata/{terminal.TerminalID.ToString().Substring(0, 8)}/logo{Path.GetExtension(file.FileName)}";

                var logoUrl = await blobStorageService.Upload(filename, uploadStream);

                terminal.PaymentRequestSettings.MerchantLogo = logoUrl;
                await terminalsService.UpdateEntity(terminal);
                response.AdditionalData = JObject.FromObject(new { logoUrl });
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("{terminalID}/resetApiKey")]
        public async Task<ActionResult<OperationResponse>> CreateTerminalApiKey([FromRoute] Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            if (!terminal.FeatureEnabled(Merchants.Shared.Enums.FeatureEnum.Api))
            {
                return BadRequest(new OperationResponse(Messages.ApiFeatureMustBeEnabled, StatusEnum.Error));
            }

            var opResult = await userManagementClient.CreateTerminalApiKey(new CreateTerminalApiKeyRequest { TerminalID = terminal.TerminalID, MerchantID = terminal.MerchantID });

            // TODO: CreateTerminalApiKey failed case
            return Ok(new OperationResponse(Messages.PrivateKeyUpdated, StatusEnum.Success, opResult.ApiKey));
        }

        // TODO: concurrency check, handle exceptions
        [HttpPost]
        [Route("{terminalID}/resetSharedApiKey")]
        public async Task<ActionResult<OperationResponse>> CreateSharedTerminalApiKey([FromRoute] Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            var sharedApiKey = cryptoServiceCompact.EncryptCompact(Guid.NewGuid().ToString());
            terminal.SharedApiKey = Convert.FromBase64String(sharedApiKey);

            await terminalsService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.SharedKeyUpdated, StatusEnum.Success, sharedApiKey));
        }

        [HttpGet]
        [Route("available-features")]
        public async Task<ActionResult<FeatureSummary>> GetAvailableFeatures()
        {
            var features = await mapper.ProjectTo<FeatureSummary>(featuresService.GetQuery()).ToListAsync();

            return Ok(features);
        }
    }
}