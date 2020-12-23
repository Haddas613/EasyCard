using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServerClient;
using Merchants.Api.Extensions;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.User;
using Merchants.Business.Services;
using Merchants.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Extensions;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/user")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class UserApiController : ApiControllerBase
    {
        private readonly IUserManagementClient userManagementClient;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;
        private readonly IMerchantsService merchantsService;

        public UserApiController(ITerminalsService terminalsService, IUserManagementClient userManagementClient, IMapper mapper, IMerchantsService merchantsService)
        {
            this.userManagementClient = userManagementClient;
            this.terminalsService = terminalsService;
            this.mapper = mapper;
            this.merchantsService = merchantsService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(UserSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(UserSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<UserSummary>>> GetUsers([FromQuery] GetUsersFilter filter)
        {
            var query = merchantsService.GetMerchantUsers().Filter(filter);

            var response = new SummariesResponse<UserSummary>
            {
                NumberOfRecords = await query.CountAsync(),
                Data = await mapper.ProjectTo<UserSummary>(query.OrderByDescending(u => u.UserTerminalMappingID)).ApplyPagination(filter).ToListAsync()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("{userID}")]
        public async Task<ActionResult<UserResponse>> GetUser([FromRoute]Guid userID)
        {
            var userEntity = EnsureExists(await userManagementClient.GetUserByID(userID));

            var userData = mapper.Map<UserResponse>(userEntity);

            return Ok(userData);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("invite")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> InviteUser([FromBody]InviteUserRequest request)
        {
            // var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(t => t.TerminalID == request.TerminalID));
            var merchant = EnsureExists(await merchantsService.GetMerchants().FirstOrDefaultAsync(t => t.MerchantID == request.MerchantID));

            var user = await userManagementClient.GetUserByEmail(request.Email);
            if (user == null)
            {
                var createUserResponse = await userManagementClient.CreateUser(mapper.Map<CreateUserRequestModel>(request));
                if (createUserResponse.ResponseCode != UserOperationResponseCodeEnum.UserCreated)
                {
                    return BadRequest(createUserResponse.Convert(correlationID: GetCorrelationID()));
                }

                user = await userManagementClient.GetUserByEmail(request.Email);

                var userToMerchantInfo = mapper.Map<UserInfo>(user);

                await merchantsService.LinkUserToMerchant(userToMerchantInfo, request.MerchantID);
            }
            else
            {
                var userIsLinkedToMerchant = (await merchantsService.GetMerchantUsers(merchant.MerchantID).CountAsync(u => u.UserID == user.UserID)) > 0;

                if (!userIsLinkedToMerchant)
                {
                    var userToMerchantInfo = mapper.Map<UserInfo>(user);

                    await merchantsService.LinkUserToMerchant(userToMerchantInfo, request.MerchantID);
                }

                var resendInvitationResponse = await userManagementClient.ResendInvitation(new ResendInvitationRequestModel { Email = user.Email });

                if (resendInvitationResponse.ResponseCode != UserOperationResponseCodeEnum.InvitationResent)
                {
                    return BadRequest(resendInvitationResponse.Convert(correlationID: GetCorrelationID()));
                }
            }

            var userInfo = new UserInfo
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Roles = request.Roles,
                UserID = user.UserID
            };

            return CreatedAtAction(nameof(GetUser), new { userID = user.UserID }, new OperationResponse(Messages.UserInvited, StatusEnum.Success, user.UserID, correlationId: GetCorrelationID()));
        }

        [HttpPost]
        [Route("{userID}/lock")]
        public async Task<ActionResult<OperationResponse>> LockUser([FromRoute]Guid userID)
        {
            var opResult = await userManagementClient.LockUser(userID);

            if (opResult.ResponseCode != UserOperationResponseCodeEnum.UserLocked)
            {
                return BadRequest(opResult.Convert(correlationID: GetCorrelationID()));
            }

            return Ok(opResult.Convert(correlationID: GetCorrelationID()));
        }

        [HttpPost]
        [Route("{userID}/unlock")]
        public async Task<ActionResult<OperationResponse>> UnLockUser([FromRoute]Guid userID)
        {
            var opResult = await userManagementClient.UnLockUser(userID);

            if (opResult.ResponseCode != UserOperationResponseCodeEnum.UserUnlocked)
            {
                return BadRequest(opResult.Convert(correlationID: GetCorrelationID()));
            }

            return Ok(opResult.Convert(correlationID: GetCorrelationID()));
        }

        [HttpPost]
        [Route("{userID}/resetPassword")]
        public async Task<ActionResult<OperationResponse>> ResetPasswordForUser([FromRoute]Guid userID)
        {
            var opResult = await userManagementClient.ResetPassword(userID);

            if (opResult.ResponseCode != UserOperationResponseCodeEnum.PasswordReseted)
            {
                return BadRequest(opResult.Convert(correlationID: GetCorrelationID()));
            }

            return Ok(opResult.Convert(correlationID: GetCorrelationID()));
        }

        [HttpDelete]
        [Route("{userID}/unlinkFromMerchant/{merchantID}")]
        public async Task<ActionResult<OperationResponse>> UnlinkUserFromTerminal([FromRoute]Guid userID, [FromRoute]Guid merchantID)
        {
            _ = EnsureExists(await userManagementClient.GetUserByID(userID));

            await merchantsService.UnLinkUserFromMerchant(userID, merchantID);

            return Ok(new OperationResponse { Message = Messages.UserUnlinkedFromMerchant, Status = StatusEnum.Success });
        }
    }
}