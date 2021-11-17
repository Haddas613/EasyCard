using AutoMapper;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.System;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.System;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Logging;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using System;
using System.Threading.Tasks;
using SharedApi = Shared.Api;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/system")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class SystemApiController : ApiControllerBase
    {
        private readonly IMapper mapper;
        private readonly IDatabaseLogService databaseLogService;
        private readonly ISystemSettingsService systemSettingsService;

        public SystemApiController(IMapper mapper, IDatabaseLogService databaseLogService, ISystemSettingsService systemSettingsService)
        {
            this.mapper = mapper;
            this.databaseLogService = databaseLogService;
            this.systemSettingsService = systemSettingsService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(DatabaseLogEntry)
                    .GetObjectMeta(DatabaseLogEntryResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta-integration-logs")]
        public TableMeta GetIntegrationLogsMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(IntegrationRequestLog)
                    .GetObjectMeta(IntegrationRequestLogResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        [Route("log")]
        public async Task<ActionResult<SummariesResponse<DatabaseLogEntry>>> GetLogEntries([FromQuery] DatabaseLogQuery filter)
        {
            try
            {
                return Ok(await databaseLogService.GetLogEntries(filter));
            }
            catch (Exception ex)
            {
                return BadRequest(new OperationResponse { Status = SharedApi.Models.Enums.StatusEnum.Error, Message = ex.Message });
            }
        }

        [HttpGet]
        [Route("settings")]
        public async Task<ActionResult<SystemSettingsResponse>> GetSystemSettings()
        {
            try
            {
                return Ok(mapper.Map<SystemSettingsResponse>(await systemSettingsService.GetSystemSettings()));
            }
            catch (Exception ex)
            {
                return BadRequest(new OperationResponse { Status = SharedApi.Models.Enums.StatusEnum.Error, Message = ex.Message });
            }
        }

        [HttpPut]
        [Route("settings")]
        public async Task<ActionResult<OperationResponse>> SaveSystemSettings(SystemSettingsRequest model)
        {
            try
            {
                var settings = mapper.Map<SystemSettings>(model);
                await systemSettingsService.UpdateSystemSettings(settings);
                return Ok(new OperationResponse("System settings updated", SharedApi.Models.Enums.StatusEnum.Success));
            }
            catch (Exception ex)
            {
                return BadRequest(new OperationResponse { Status = SharedApi.Models.Enums.StatusEnum.Error, Message = ex.Message });
            }
        }
    }
}
