using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServerClient;
using Merchants.Api.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Models;
using Shared.Api.Models.Enums;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/user")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly IUserManagementClient userManagementClient;

        public UserApiController(IUserManagementClient userManagementClient)
        {
            this.userManagementClient = userManagementClient;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        [Route("{userID}")]
        public async Task<IActionResult> GetUser([FromRoute]string userID)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}")]
        public async Task<IActionResult> UpdateUser([FromRoute]string userEmail, [FromBody]UpdateUserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/invite")]
        public async Task<IActionResult> InviteUser([FromRoute]string userEmail, [FromBody]InviteUserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/lock")]
        public async Task<IActionResult> LockUser([FromRoute]string userEmail)
        {
            var user = await userManagementClient.GetUserByEmail(userEmail);

            var opResult = await userManagementClient.LockUser(user.EntityReference);

            return new JsonResult(new OperationResponse { Message = "ok", Status = StatusEnum.Success }) { StatusCode = 200 };
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/unlock")]
        public async Task<IActionResult> UnLockUser([FromRoute]string userEmail)
        {
            var user = await userManagementClient.GetUserByEmail(userEmail);

            var opResult = await userManagementClient.UnLockUser(user.EntityReference);

            return new JsonResult(new OperationResponse { Message = "ok", Status = StatusEnum.Success }) { StatusCode = 200 };
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/resetPassword")]
        public async Task<IActionResult> ResetPasswordForUser([FromRoute]string userEmail)
        {
            var user = await userManagementClient.GetUserByEmail(userEmail);

            var opResult = await userManagementClient.ResetPassword(user.EntityReference);

            return new JsonResult(new OperationResponse { Message = "ok", Status = StatusEnum.Success }) { StatusCode = 200 };
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/linkToTerminal/{terminalID}")]
        public async Task<IActionResult> LinkUserToTerminal([FromRoute]string userEmail, [FromRoute]long terminalID)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/unlinkFromTerminal/{terminalID}")]
        public async Task<IActionResult> UnlinkUserFromTerminal([FromRoute]string userEmail, [FromRoute]long terminalID)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [Route("{userID}/unlinkFromMerchant/{terminalID}")]
        [Obsolete("Users will be linked to merchant by terminals")]
        public async Task<IActionResult> UnlinkUserFromMerchant([FromRoute]string userEmail, [FromRoute]long merchantID)
        {
            throw new NotImplementedException();
        }
    }
}