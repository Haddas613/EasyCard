using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BasicServices.BlobStorage;
using EasyInvoice;
using IdentityServerClient;
using MerchantProfileApi.Extensions;
using MerchantProfileApi.Models.Terminal;
using Merchants.Business.Extensions;
using Merchants.Business.Services;
using Merchants.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProfileApi;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers.Security;
using Shared.Integration;
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
        private readonly ILogger logger;
        private readonly IExternalSystemsService externalSystemsService;

        //TODO: temporary, use events to update EC logo
        private readonly ECInvoiceInvoicing eCInvoiceInvoicing;

        public TerminalsApiController(
            IMerchantsService merchantsService,
            ITerminalsService terminalsService,
            IMapper mapper,
            IUserManagementClient userManagementClient,
            ISystemSettingsService systemSettingsService,
            ICryptoServiceCompact cryptoServiceCompact,
            IFeaturesService featuresService,
            IBlobStorageService blobStorageService,
            IExternalSystemsService externalSystemsService,
            ILogger<TerminalsApiController> logger,
            ECInvoiceInvoicing eCInvoiceInvoicing)
        {
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
            this.mapper = mapper;
            this.userManagementClient = userManagementClient;
            this.systemSettingsService = systemSettingsService;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.featuresService = featuresService;
            this.logger = logger;
            this.blobStorageService = blobStorageService;
            this.eCInvoiceInvoicing = eCInvoiceInvoicing;
            this.externalSystemsService = externalSystemsService;
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
            var entity = EnsureExists(await terminalsService.GetTerminal(terminalID));
            var terminal = mapper.Map<TerminalResponse>(entity);

            var merchant = EnsureExists(await merchantsService.GetMerchants().FirstOrDefaultAsync(m => m.MerchantID == terminal.MerchantID));

            terminal.MerchantName = merchant.BusinessName;

            var systemSettings = await systemSettingsService.GetSystemSettings();

            mapper.Map(systemSettings, terminal);

            if (entity.Integrations?.Any() == true)
            {
                var externalSystemNames = externalSystemsService.GetExternalSystems().ToDictionary(k => k.ExternalSystemID, v => v.Name);
                terminal.Integrations = entity.Integrations.ToDictionary(k => k.Type, v => externalSystemNames.ContainsKey(v.ExternalSystemID) ? externalSystemNames[v.ExternalSystemID] : string.Empty);
            }

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

        //[RequestSizeLimit(1000000)]
        [HttpPost]
        [Route("{terminalID}/merchantlogo")]
        [Consumes("multipart/form-data")]
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

            var response = new OperationResponse { Message = Messages.ImageSaved, Status = StatusEnum.Success };

            using (var uploadStream = file.OpenReadStream())
            {
                uploadStream.Seek(0, SeekOrigin.Begin);

                var filename = $"merchantdata/{terminal.TerminalID.ToString().Substring(0, 8)}/logo{Path.GetExtension(file.FileName)}";

                var url = await blobStorageService.Upload(filename, uploadStream, file.ContentType);

                terminal.PaymentRequestSettings.MerchantLogo = url;
                await terminalsService.UpdateEntity(terminal);
                response.AdditionalData = JObject.FromObject(new { url });

                //TODO: temporary, use events to update EC logo
                var easyInvoiceIntegration = terminal.Integrations.FirstOrDefault(i => i.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID);
                if (easyInvoiceIntegration != null)
                {
                    using var memoryStream = new MemoryStream();

                    uploadStream.Seek(0, SeekOrigin.Begin);
                    await uploadStream.CopyToAsync(memoryStream);
                    var ecTerminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();
                    try
                    {
                        var uploadOperation = await eCInvoiceInvoicing.UploadUserLogo(ecTerminalSettings, memoryStream, file.FileName, GetCorrelationID());

                        if (uploadOperation.Status != StatusEnum.Success)
                        {
                            logger.LogError($"Error while uploading logo to EasyInvoice: {uploadOperation.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error while uploading logo to EasyInvoice: {ex.Message}");
                    }
                }
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("{terminalID}/customcss")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<OperationResponse>> UploadCustomCss([FromRoute]Guid terminalID, [FromForm]IFormFile file)
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

            if (!file.ContentType.Contains("text/css"))
            {
                return BadRequest(new OperationResponse { Message = Messages.OnlyCSSFilesAreAllowed, Status = StatusEnum.Error });
            }

            var response = new OperationResponse { Message = Messages.Saved, Status = StatusEnum.Success };

            using (var uploadStream = file.OpenReadStream())
            {
                uploadStream.Seek(0, SeekOrigin.Begin);

                var filename = $"merchantdata/{terminal.TerminalID.ToString().Substring(0, 8)}/style.css";

                var url = await blobStorageService.Upload(filename, uploadStream, file.ContentType);

                terminal.CheckoutSettings.CustomCssReference = url;
                await terminalsService.UpdateEntity(terminal);
                response.AdditionalData = JObject.FromObject(new { url });
            }

            return Ok(response);
        }

        [HttpDelete]
        [Route("{terminalID}/customcss")]
        public async Task<ActionResult<OperationResponse>> DeleteCustomCss([FromRoute]Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));
            EnsureExists(terminal.CheckoutSettings?.CustomCssReference);

            terminal.CheckoutSettings.CustomCssReference = null;
            await terminalsService.UpdateEntity(terminal);

            var response = new OperationResponse { Message = Messages.DeletedSuccessfully, Status = StatusEnum.Success };
            return Ok(response);
        }

        [HttpDelete]
        [Route("{terminalID}/merchantlogo")]
        public async Task<ActionResult<OperationResponse>> DeleteMerchantLogo([FromRoute]Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));
            EnsureExists(terminal.PaymentRequestSettings?.MerchantLogo);

            terminal.PaymentRequestSettings.MerchantLogo = null;
            await terminalsService.UpdateEntity(terminal);

            var response = new OperationResponse { Message = Messages.DeletedSuccessfully, Status = StatusEnum.Success };
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

        [HttpGet]
        [Route("terminal-devices/{terminalID:guid}")]
        public async Task<ActionResult<IEnumerable<TerminalDeviceResponse>>> GetTerminalPinPadDevices(Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));
            var integration = terminal.Integrations.FirstOrDefault(t => t.Type == ExternalSystemTypeEnum.PinpadProcessor);
            var response = new List<TerminalDeviceResponse>();

            //TODO: temporary
            if (integration?.Settings != null && integration?.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID)
            {
                var devices = integration.Settings.GetValue("devices")?.ToObject<IEnumerable<JObject>>();
                if (devices != null)
                {
                    foreach (var d in devices)
                    {
                        response.Add(new TerminalDeviceResponse { DeviceID = d.GetValue("terminalID").Value<string>(), DeviceName = d.GetValue("posName").Value<string>() });
                    }
                }
            }

            return Ok(response);
        }
    }
}