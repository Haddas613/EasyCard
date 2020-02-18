using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MerchantsApi.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace MerchantsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/user")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummariesResponse<UserSummary>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        public async Task<IActionResult> GetUsers(GetUsersFilter filter)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
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
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(OperationResponse))]
        public async Task<IActionResult> CreateUser([FromBody]UserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(OperationResponse))]
        [Route("{userID}")]
        public async Task<IActionResult> UpdateUser([FromRoute]string userID, [FromBody]UpdateUserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{userID}/invite")]
        public async Task<IActionResult> InviteUser([FromRoute]string userID, [FromBody]InviteUserRequest user)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{userID}/lock")]
        public async Task<IActionResult> LockUser([FromRoute]string userID)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{userID}/resetPassword")]
        public async Task<IActionResult> ResetPasswordForUser([FromRoute]string userID)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{userID}/linkToTerminal/{terminalID}")]
        public async Task<IActionResult> LinkUserToTerminal([FromRoute]string userID, [FromRoute]long terminalID)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{userID}/unlinkFromTerminal/{terminalID}")]
        public async Task<IActionResult> UnlinkUserFromTerminal([FromRoute]string userID, [FromRoute]long terminalID)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{userID}/unlinkFromMerchant/{terminalID}")]
        public async Task<IActionResult> UnlinkUserFromMerchant([FromRoute]string userID, [FromRoute]long merchantID)
        {
            throw new NotImplementedException();
        }
    }
}