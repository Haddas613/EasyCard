using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServerClient;
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
        public async Task<ActionResult<UserResponse>> GetUsers([FromQuery] GetUsersFilter filter)
        {
            throw new NotImplementedException();
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
                // TODO: convert non-success response
                _ = await userManagementClient.CreateUser(mapper.Map<CreateUserRequestModel>(request));
                user = await userManagementClient.GetUserByEmail(request.Email);
            }
            else
            {
                // TODO: convert non-success response
                _ = await userManagementClient.ResendInvitation(new ResendInvitationRequestModel { Email = user.Email });
            }

            var userInfo = new UserInfo
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Roles = request.Roles,
                UserID = user.UserID
            };

            await terminalsService.LinkUserToTerminal(userInfo, terminal);

            return CreatedAtAction(nameof(GetUser), new { userID = user.UserID }, new OperationResponse(Messages.UserInvited, StatusEnum.Success, user.UserID.ToString()));
        }

        [HttpPost]
        [Route("{userID}/lock")]
        public async Task<ActionResult<OperationResponse>> LockUser([FromRoute]Guid userID)
        {
            var opResult = await userManagementClient.LockUser(userID);

            // TODO: convert non-success response
            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }

        [HttpPost]
        [Route("{userID}/unlock")]
        public async Task<ActionResult<OperationResponse>> UnLockUser([FromRoute]Guid userID)
        {
            var opResult = await userManagementClient.UnLockUser(userID);

            // TODO: convert non-success response
            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }

        [HttpPost]
        [Route("{userID}/resetPassword")]
        public async Task<ActionResult<OperationResponse>> ResetPasswordForUser([FromRoute]Guid userID)
        {
            var opResult = await userManagementClient.ResetPassword(userID);

            // TODO: convert non-success response
            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }

        [HttpPut]
        [Route("{userID}/linkToTerminal/{terminalID}")]
        public async Task<ActionResult<OperationResponse>> LinkUserToTerminal([FromBody] LinkUserToTerminalRequest request)
        {
            var user = EnsureExists(await userManagementClient.GetUserByID(request.UserID));

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

            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }

        [HttpDelete]
        [Route("{userID}/unlinkFromTerminal/{terminalID}")]
        public async Task<ActionResult<OperationResponse>> UnlinkUserFromTerminal([FromRoute]Guid userID, [FromRoute]Guid terminalID)
        {
            //Check if user exists
            _ = EnsureExists(await userManagementClient.GetUserByID(userID));

            await terminalsService.UnLinkUserFromTerminal(userID, terminalID);

            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }
    }
}