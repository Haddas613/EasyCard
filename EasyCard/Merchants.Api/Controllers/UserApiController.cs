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
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Extensions;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/user")]
    [ApiController]
    public class UserApiController : ControllerBase
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        [Route("{userID}")]
        public async Task<IActionResult> GetUser([FromRoute]string userID)
        {
            var userData = mapper.Map<UserResponse>(await userManagementClient.GetUserByID(userID).EnsureExists());

            return new JsonResult(userData) { StatusCode = 200 };
        }

        /// <summary>
        /// 409 response means that user with given email already exists
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<IActionResult> CreateUser([FromBody]UserRequest user)
        {
            var opResult = await userManagementClient.CreateUser(mapper.Map<CreateUserRequestModel>(user));
            return new JsonResult(new OperationResponse("ok", StatusEnum.Success, opResult.EntityReference)) { StatusCode = 201 };
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userEmail}")]
        [Obsolete("Candidate for removal")]
        public async Task<IActionResult> UpdateUser([FromRoute]string userEmail, [FromBody]UpdateUserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("invite")]
        public async Task<IActionResult> InviteUser([FromBody]InviteUserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/lock")]
        public async Task<IActionResult> LockUser([FromRoute]string userID)
        {
            var opResult = await userManagementClient.LockUser(userID);

            return new JsonResult(new OperationResponse { Message = "ok", Status = StatusEnum.Success }) { StatusCode = 200 };
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/unlock")]
        public async Task<IActionResult> UnLockUser([FromRoute]string userID)
        { 
            var opResult = await userManagementClient.UnLockUser(userID);

            return new JsonResult(new OperationResponse { Message = "ok", Status = StatusEnum.Success }) { StatusCode = 200 };
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/resetPassword")]
        public async Task<IActionResult> ResetPasswordForUser([FromRoute]string userID)
        {
            var opResult = await userManagementClient.ResetPassword(userID);

            return new JsonResult(new OperationResponse { Message = "ok", Status = StatusEnum.Success }) { StatusCode = 200 };
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/linkToTerminal/{terminalID}")]
        public async Task<IActionResult> LinkUserToTerminal([FromRoute]string userID, [FromRoute]long terminalID)
        {
            //Check if user exists
            var userData = mapper.Map<UserResponse>(await userManagementClient.GetUserByID(userID).EnsureExists());

            await terminalsService.LinkUserToTerminal(userID, terminalID);

            return new JsonResult(new OperationResponse { Message = "ok", Status = StatusEnum.Success }) { StatusCode = 200 };
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/unlinkFromTerminal/{terminalID}")]
        public async Task<IActionResult> UnlinkUserFromTerminal([FromRoute]string userID, [FromRoute]long terminalID)
        {
            //Check if user exists
            var userData = mapper.Map<UserResponse>(await userManagementClient.GetUserByID(userID).EnsureExists());

            await terminalsService.UnLinkUserFromTerminal(userID, terminalID);

            return new JsonResult(new OperationResponse { Message = "ok", Status = StatusEnum.Success }) { StatusCode = 200 };
        }
    }
}