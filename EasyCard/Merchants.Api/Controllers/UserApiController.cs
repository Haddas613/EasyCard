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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Obsolete("Move To: Invite User")]
        public async Task<ActionResult<OperationResponse>> CreateUser([FromBody]UserRequest user)
        {
            var opResult = await userManagementClient.CreateUser(mapper.Map<CreateUserRequestModel>(user));
            return CreatedAtAction(nameof(GetUser), new { userID = opResult.EntityReference }, new OperationResponse("ok", StatusEnum.Success, opResult.EntityReference));
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userEmail}")]
        [Obsolete("Candidate for removal")]
        public async Task<ActionResult<OperationResponse>> UpdateUser([FromRoute]string userEmail, [FromBody]UpdateUserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("invite")]
        public async Task<ActionResult<OperationResponse>> InviteUser([FromBody]InviteUserRequest user)
        {
            throw new NotImplementedException();
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