using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServerClient;
using Merchants.Api.Models.User;
using Merchants.Business.Services;
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
        [Route("{userID}")]
        public async Task<ActionResult<UserResponse>> GetUser([FromRoute]string userID)
        {
            var userData = mapper.Map<UserResponse>(EnsureExists(await userManagementClient.GetUserByID(userID)));

            return Ok(userData);
        }

        [HttpPut]
        [Route("{userEmail}")]
        [Obsolete("Candidate for removal")]
        public async Task<ActionResult<OperationResponse>> UpdateUser([FromRoute]string userEmail, [FromBody]UpdateUserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("invite")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> InviteUser([FromBody]InviteUserRequest request)
        {
            _ = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(t => t.TerminalID == request.TerminalID));

            var user = await userManagementClient.GetUserByEmail(request.Email);
            if (user == null)
            {
                _ = await userManagementClient.CreateUser(mapper.Map<CreateUserRequestModel>(request));
                user = await userManagementClient.GetUserByEmail(request.Email);
            }

            await terminalsService.LinkUserToTerminal(user.UserID, request.TerminalID);

            return CreatedAtAction(nameof(GetUser), new { userID = user.UserID }, new OperationResponse("ok", StatusEnum.Success, user.UserID));
        }

        [HttpPost]
        [Route("{userID}/lock")]
        public async Task<ActionResult<OperationResponse>> LockUser([FromRoute]string userID)
        {
            var opResult = await userManagementClient.LockUser(userID);

            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }

        [HttpPost]
        [Route("{userID}/unlock")]
        public async Task<ActionResult<OperationResponse>> UnLockUser([FromRoute]string userID)
        {
            var opResult = await userManagementClient.UnLockUser(userID);

            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }

        [HttpPost]
        [Route("{userID}/resetPassword")]
        public async Task<ActionResult<OperationResponse>> ResetPasswordForUser([FromRoute]string userID)
        {
            var opResult = await userManagementClient.ResetPassword(userID);

            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }

        [HttpPut]
        [Route("{userID}/linkToTerminal/{terminalID}")]
        public async Task<ActionResult<OperationResponse>> LinkUserToTerminal([FromRoute]string userID, [FromRoute]long terminalID)
        {
            //Check if user exists
            _ = EnsureExists(await userManagementClient.GetUserByID(userID));

            await terminalsService.LinkUserToTerminal(userID, terminalID);

            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }

        [HttpDelete]
        [Route("{userID}/unlinkFromTerminal/{terminalID}")]
        public async Task<ActionResult<OperationResponse>> UnlinkUserFromTerminal([FromRoute]string userID, [FromRoute]long terminalID)
        {
            //Check if user exists
            _ = EnsureExists(await userManagementClient.GetUserByID(userID));

            await terminalsService.UnLinkUserFromTerminal(userID, terminalID);

            return Ok(new OperationResponse { Message = "ok", Status = StatusEnum.Success });
        }
    }
}