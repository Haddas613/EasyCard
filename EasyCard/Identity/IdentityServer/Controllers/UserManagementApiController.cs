using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.Data;
using IdentityServer.Helpers;
using IdentityServer.Models;
using IdentityServerClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers.Email;
using Shared.Helpers.Security;

namespace IdentityServer.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/userManagement")]
    [Authorize(Policy = Policy.ManagementApi, AuthenticationSchemes = "token")]
    public class UserManagementApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger logger;
        private readonly ApplicationDbContext dataContext;
        private readonly IEmailSender emailSender;
        private readonly ICryptoService cryptoService;
        private readonly ApplicationSettings configuration;

        public UserManagementApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dataContext,
            ILogger<UserManagementApiController> logger,
            IEmailSender emailSender,
            ICryptoService cryptoService,
            IOptions<ApplicationSettings> configuration
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.dataContext = dataContext;
            this.emailSender = emailSender;
            this.cryptoService = cryptoService;
            this.configuration = configuration?.Value;
        }

        /// <summary>
        /// Get User by ID
        /// </summary>
        /// <param name="userId"></param>
        [HttpGet]
        [Route("user/{userId}")]
        public async Task<ActionResult<UserProfileDataResponse>> GetUserByID([FromRoute]string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            return await GetUser(user);
        }

        /// <summary>
        /// Get User by Email
        /// </summary>
        /// <param name="email"></param>
        [HttpGet]
        [Route("user")]
        public async Task<ActionResult<UserProfileDataResponse>> GetUserByEmail([FromQuery]string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            return await GetUser(user);
        }

        // TODO: validate model

        /// <summary>
        /// Create user
        /// </summary>
        /// <param name="model">Create User Request</param>
        /// <returns>Operation Response</returns>
        [HttpPost]
        [Route("user")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<UserOperationResponse>> CreateUser([FromBody]CreateUserRequestModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                return Conflict(new UserOperationResponse { ResponseCode = UserOperationResponseCodeEnum.UserAlreadyExists });
            }

            if (user == null)
            {
                user = new ApplicationUser { UserName = model.Email, Email = model.Email, /*PhoneNumber = model.CellPhone*/ };
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    logger.LogInformation("User is not created");

                    // TODO: error details
                    return new BadRequestResult();
                }

                logger.LogInformation("User created a new account");
            }

            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = cryptoService.EncryptWithExpiration(user.Id, TimeSpan.FromHours(configuration.ConfirmationEmailExpirationInHours));

            var callbackUrl = Url.EmailConfirmationLink(code, Request.Scheme);
            await emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

            var allClaims = await userManager.GetClaimsAsync(user);

            //await _userManager.AddClaim(allClaims, user, "extension_PaymentGatewayID", model.PaymentGatewayID.ToString());
            //await _userManager.AddClaim(allClaims, user, "extension_MerchantID", model.MerchantID.ToString());
            //await _userManager.AddClaim(allClaims, user, "extension_FirstName", model.FirstName);
            //await _userManager.AddClaim(allClaims, user, "extension_LastName", model.LastName);

            await userManager.AddToRoleAsync(user, "Merchant");

            var operationResult = new UserOperationResponse
            {
                UserID = user.Id,
                ResponseCode = UserOperationResponseCodeEnum.UserCreated
            };

            // TODO: config
            await userManager.SetTwoFactorEnabledAsync(user, true);

            return new ObjectResult(operationResult);
        }

        // TODO: validate model
        [HttpPost]
        [Route("terminal")]
        public async Task<IActionResult> AddTerminal([FromBody]CreateUserRequestModel model)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return new ObjectResult("User does not exist") { StatusCode = 404 };
                }

                var allClaims = await userManager.GetClaimsAsync(user);

                //await _userManager.AddClaim(allClaims, user, "extension_MerchantID", model.MerchantID.ToString(), true);

                var operationResult = new UserOperationResponse
                {
                    UserID = user.Id,
                    ResponseCode = UserOperationResponseCodeEnum.UserUpdated
                };

                // TODO: config
                await userManager.SetTwoFactorEnabledAsync(user, true);

                return new ObjectResult(operationResult);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Cannot process user {model.Email}");
                return StatusCode(500);
            }
        }

        [HttpDelete]
        [Route("user/{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute]string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID is required");
            }

            var user = userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return NotFound($"User {userId} does not exist");
            }

            var operationResult = new UserOperationResponse
            {
                UserID = user.Id,
                ResponseCode = UserOperationResponseCodeEnum.UserDeleted
            };

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // TODO
                //operationResult.Errors = result.Errors
                //    .Select(i => new Error { Code = i.Code, Description = i.Description })
                //    .ToList();

                return new ObjectResult(operationResult) { StatusCode = 400 };
            }

            return new ObjectResult(operationResult);
        }

        [HttpPost]
        [Route("user/{userId}/resetPassword")]
        public async Task<IActionResult> ResetPassword([FromRoute]string userId)
        {
            var user = userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return NotFound($"User {userId} does not exist");
            }

            var code = cryptoService.EncryptWithExpiration(user.Id, TimeSpan.FromHours(configuration.ResetPasswordEmailExpirationInHours));

            var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);

            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, Guid.NewGuid().ToString());

            var result = await userManager.UpdateAsync(user);

            //if (!result.Succeeded)

            var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                logger.LogError($"Unexpected error occurred disabling 2FA for user with ID '{user.Id}'.");
            }

            await emailSender.SendEmailResetPasswordAsync(user.Email, callbackUrl);

            var operationResult = new UserOperationResponse
            {
                UserID = user.Id,
                ResponseCode = UserOperationResponseCodeEnum.PasswordReseted
            };

            return new ObjectResult(operationResult);
        }

        [HttpPost]
        [Route("user/{userId}/lock")]
        public async Task<IActionResult> Lock([FromRoute]string userId)
        {
            var user = userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return NotFound($"User {userId} does not exist");
            }

            var res = await userManager.SetLockoutEnabledAsync(user, true);

            if (!res.Succeeded)
            {
                // TODO
                return new JsonResult(new { Status = "error", Errors = res.Errors }) { StatusCode = 500 };
            }

            res = await userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));

            if (!res.Succeeded)
            {
                // TODO
                return new JsonResult(new { Status = "error", Errors = res.Errors }) { StatusCode = 500 };
            }

            // TODO
            return new JsonResult(new { Status = "success" });
        }

        [HttpPost]
        [Route("user/{userId}/unlock")]
        public async Task<IActionResult> UnLock([FromRoute]string userId)
        {
            var user = userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return NotFound($"User {userId} does not exist");
            }

            var res = await userManager.SetLockoutEnabledAsync(user, false);

            if (!res.Succeeded)
            {
                // TODO
                return new JsonResult(new { Status = "error", Errors = res.Errors }) { StatusCode = 500 };
            }

            res = await userManager.SetLockoutEndDateAsync(user, null);

            if (!res.Succeeded)
            {
                // TODO
                return new JsonResult(new { Status = "error", Errors = res.Errors }) { StatusCode = 500 };
            }

            // TODO
            return new JsonResult(new { Status = "success" });
        }

        private async Task<ActionResult<UserProfileDataResponse>> GetUser(ApplicationUser user)
        {
            if (user == null)
            {
                return new NotFoundObjectResult(new { Status = "error", Message = "User does not exist" });
            }

            var result = new UserProfileDataResponse
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserID = user.Id,
                Roles = await userManager.GetRolesAsync(user)
            };

            var allClaims = await userManager.GetClaimsAsync(user);

            return Ok(result);
        }
    }
}