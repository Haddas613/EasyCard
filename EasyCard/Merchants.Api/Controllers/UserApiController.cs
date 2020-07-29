using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServerClient;
using Merchants.Api.Extensions;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.User;
using Merchants.Business.Services;
using Merchants.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
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

        public UserApiController(ITerminalsService terminalsService, IUserManagementClient userManagementClient, IMapper mapper)
        {
            this.userManagementClient = userManagementClient;
            this.terminalsService = terminalsService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers([FromQuery] GetUsersFilter filter)
        {
            var userEntity = EnsureExists(await userManagementClient.GetUserByEmail(filter.Email));

            var userData = mapper.Map<UserResponse>(userEntity);

            userData.Terminals = (await terminalsService.GetUserTerminals(userEntity.UserID).ToListAsync())
                .Select(d => mapper.Map<Models.Terminal.TerminalSummary>(d));

            return Ok(new List<UserResponse> { userData });
        }

        [HttpGet]
        [Route("{userID}")]
        public async Task<ActionResult<UserResponse>> GetUser([FromRoute]Guid userID)
        {
            var userEntity = EnsureExists(await userManagementClient.GetUserByID(userID));

            var userData = mapper.Map<UserResponse>(userEntity);

            userData.Terminals = (await terminalsService.GetUserTerminals(userID).ToListAsync())
                .Select(d => mapper.Map<Models.Terminal.TerminalSummary>(d));

            return Ok(userData);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("invite")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> InviteUser([FromBody]InviteUserRequest request)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(t => t.TerminalID == request.TerminalID));

            var user = await userManagementClient.GetUserByEmail(request.Email);
            if (user == null)
            {
                var createUserResponse = await userManagementClient.CreateUser(mapper.Map<CreateUserRequestModel>(request));
                if (createUserResponse.ResponseCode != UserOperationResponseCodeEnum.UserCreated)
                {
                    return BadRequest(createUserResponse.Convert(correlationID: GetCorrelationID()));
                }

                user = await userManagementClient.GetUserByEmail(request.Email);
            }
            else
            {
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

            await terminalsService.LinkUserToTerminal(userInfo, terminal);

            return CreatedAtAction(nameof(GetUser), new { userID = user.UserID }, new OperationResponse(Messages.UserInvited, StatusEnum.Success, user.UserID.ToString(), correlationId: GetCorrelationID()));
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

        [HttpPut]
        [Route("{userID}/linkToTerminal")]
        public async Task<ActionResult<OperationResponse>> LinkUserToTerminal([FromRoute]Guid userID, [FromBody] LinkUserToTerminalRequest request)
        {
            var user = EnsureExists(await userManagementClient.GetUserByID(userID));

            var terminal = EnsureExists(await terminalsService.GetTerminals()
                .FirstOrDefaultAsync(m => m.TerminalID == request.TerminalID));

            var userInfo = new UserInfo
            {
                 DisplayName = user.DisplayName,
                 Email = user.Email,
                 Roles = request.Roles,
                 UserID = user.UserID
            };

            await terminalsService.LinkUserToTerminal(userInfo, terminal);

            return Ok(new OperationResponse { Message = Messages.UserLinkedToTerminal, Status = StatusEnum.Success });
        }

        [HttpDelete]
        [Route("{userID}/unlinkFromTerminal/{terminalID}")]
        public async Task<ActionResult<OperationResponse>> UnlinkUserFromTerminal([FromRoute]Guid userID, [FromRoute]Guid terminalID)
        {
            _ = EnsureExists(await userManagementClient.GetUserByID(userID));

            await terminalsService.UnLinkUserFromTerminal(userID, terminalID);

            return Ok(new OperationResponse { Message = Messages.UserUnlinkedFromTerminal, Status = StatusEnum.Success });
        }
    }
}