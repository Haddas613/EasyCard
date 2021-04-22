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
using IdentityServer.Services;
using IdentityServerClient;
using Merchants.Api.Client;
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
        private readonly IMerchantsApiClient merchantsApiClient;
        private readonly UserManageService userManageService;

        public UserManagementApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dataContext,
            ILogger<UserManagementApiController> logger,
            IEmailSender emailSender,
            ICryptoService cryptoService,
            IOptions<ApplicationSettings> configuration,
            IMerchantsApiClient merchantsApiClient,
            UserManageService userManageService
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.dataContext = dataContext;
            this.emailSender = emailSender;
            this.cryptoService = cryptoService;
            this.configuration = configuration?.Value;
            this.merchantsApiClient = merchantsApiClient;
            this.userManageService = userManageService;
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
                user = new ApplicationUser { UserName = model.Email, Email = model.Email, /*PhoneNumber = model.CellPhone*/ LockoutEnabled = true };
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    logger.LogInformation("User is not created");

                    // TODO: error details
                    return new BadRequestResult();
                }

                var allClaims = await userManager.GetClaimsAsync(user);

                await userManager.AddClaim(allClaims, user, Claims.MerchantIDClaim, model.MerchantID);

                if (model.Roles == null)
                {
                    model.Roles = new List<string>();
                }

                if (!model.Roles.Any(r => r != Roles.Merchant))
                {
                    model.Roles.Add(Roles.Merchant);
                }

                foreach (var role in model.Roles.Distinct())
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                logger.LogInformation("User created a new account");
            }

            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = cryptoService.EncryptWithExpiration(user.Id, TimeSpan.FromHours(configuration.ConfirmationEmailExpirationInHours));

            var callbackUrl = Url.EmailConfirmationLink(code, Request?.Scheme ?? "https");
            await emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

            //await _userManager.AddClaim(allClaims, user, "extension_FirstName", model.FirstName);
            //await _userManager.AddClaim(allClaims, user, "extension_LastName", model.LastName);

            var operationResult = new UserOperationResponse
            {
                UserID = new Guid(user.Id),
                ResponseCode = UserOperationResponseCodeEnum.UserCreated
            };

            // TODO: config
            // await userManager.SetTwoFactorEnabledAsync(user, true);

            return new ObjectResult(operationResult);
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="model">Update User Request</param>
        /// <returns>Operation Response</returns>
        [HttpPut]
        [Route("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UserOperationResponse>> UpdateUser([FromBody]UpdateUserRequestModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserID);

            if (user == null)
            {
                return NotFound(new UserOperationResponse { ResponseCode = UserOperationResponseCodeEnum.UserNotFound });
            }

            var result = await userManageService.UpdateUser(model);

            if (!result)
            {
                logger.LogInformation("User is not upated");

                // TODO: error details
                return new BadRequestResult();
            }

            var operationResult = new UserOperationResponse
            {
                UserID = new Guid(user.Id),
                ResponseCode = result ? UserOperationResponseCodeEnum.UserUpdated : UserOperationResponseCodeEnum.UnknwnError
            };

            return new ObjectResult(operationResult);
        }

        [HttpPost]
        [Route("resendInvitation")]
        public async Task<ActionResult<UserOperationResponse>> ResendInvitation([FromBody]ResendInvitationRequestModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound(new UserOperationResponse { ResponseCode = UserOperationResponseCodeEnum.UserNotFound });
            }

            var isAdmin = await userManager.IsInRoleAsync(user, Roles.BillingAdministrator) || await userManager.IsInRoleAsync(user, Roles.BusinessAdministrator);

            if (isAdmin)
            {
                return Conflict(new UserOperationResponse { ResponseCode = UserOperationResponseCodeEnum.UserAlreadyExists, Message = "User with the same email is already registered" });
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                var unlockRes = await userManager.SetLockoutEndDateAsync(user, null);

                if (!unlockRes.Succeeded)
                {
                    return BadRequest(new UserOperationResponse
                    {
                        UserID = new Guid(user.Id),
                        ResponseCode = UserOperationResponseCodeEnum.UnknwnError,
                        Message = unlockRes.Errors.FirstOrDefault()?.Description
                    });
                }
            }

            if (user.PasswordHash != null)
            {
                //TODO: send new merchant available email
                return Conflict(new UserOperationResponse { ResponseCode = UserOperationResponseCodeEnum.UserAlreadyExists, Message = "User with the same email is already registered" });
            }
            else
            {
                var code = cryptoService.EncryptWithExpiration(user.Id, TimeSpan.FromHours(configuration.ConfirmationEmailExpirationInHours));

                var callbackUrl = Url.EmailConfirmationLink(code, Request.Scheme);
                await emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);
            }

            if (!string.IsNullOrEmpty(model.MerchantID))
            {
                var allClaims = await userManager.GetClaimsAsync(user);

                var merchantClaim = allClaims.FirstOrDefault(c => c.Type == Claims.MerchantIDClaim && c.Value == model.MerchantID);

                if (merchantClaim == null)
                {
                    await userManager.AddClaim(allClaims, user, Claims.MerchantIDClaim, model.MerchantID);
                }
            }

            var operationResult = new UserOperationResponse
            {
                UserID = new Guid(user.Id),
                ResponseCode = UserOperationResponseCodeEnum.InvitationResent
            };

            return new ObjectResult(operationResult);
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
                UserID = new Guid(user.Id),
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

            var callbackUrl = Url.ResetPasswordCallbackLink(code, Request.Scheme);

            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, Guid.NewGuid().ToString());

            var res = await userManager.UpdateAsync(user);

            if (!res.Succeeded)
            {
                return BadRequest(new UserOperationResponse
                {
                    UserID = new Guid(user.Id),
                    ResponseCode = UserOperationResponseCodeEnum.UnknwnError,
                    Message = res.Errors.FirstOrDefault()?.Description
                });
            }

            var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                logger.LogError($"Unexpected error occurred disabling 2FA for user with ID '{user.Id}'.");
            }

            // Unlock user
            res = await userManager.SetLockoutEnabledAsync(user, false);
            res = await userManager.SetLockoutEndDateAsync(user, null);

            await emailSender.SendEmailResetPasswordAsync(user.Email, callbackUrl);

            return Ok(new UserOperationResponse
            {
                UserID = new Guid(user.Id),
                ResponseCode = UserOperationResponseCodeEnum.PasswordReseted,
                Message = "Password reseted"
            });
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
                return BadRequest(new UserOperationResponse
                {
                    UserID = new Guid(user.Id),
                    ResponseCode = UserOperationResponseCodeEnum.UnknwnError,
                    Message = res.Errors.FirstOrDefault()?.Description
                });
            }

            await merchantsApiClient.LogUserActivity(new Merchants.Api.Client.Models.UserActivityRequest
            {
                UserActivity = Merchants.Shared.Enums.UserActivityEnum.Locked,
                UserID = user.Id,
                DisplayName = user.UserName,
                Email = user.Email
            });

            res = await userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));

            if (!res.Succeeded)
            {
                return BadRequest(new UserOperationResponse
                {
                    UserID = new Guid(user.Id),
                    ResponseCode = UserOperationResponseCodeEnum.UnknwnError,
                    Message = res.Errors.FirstOrDefault()?.Description
                });
            }

            return Ok(new UserOperationResponse
            {
                UserID = new Guid(user.Id),
                ResponseCode = UserOperationResponseCodeEnum.UserLocked,
                Message = "User account locked"
            });
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

            var res = await userManager.SetLockoutEndDateAsync(user, null);

            if (!res.Succeeded)
            {
                return BadRequest(new UserOperationResponse
                {
                    UserID = new Guid(user.Id),
                    ResponseCode = UserOperationResponseCodeEnum.UnknwnError,
                    Message = res.Errors.FirstOrDefault()?.Description
                });
            }

            await merchantsApiClient.LogUserActivity(new Merchants.Api.Client.Models.UserActivityRequest
            {
                UserActivity = Merchants.Shared.Enums.UserActivityEnum.Unlocked,
                UserID = user.Id,
                DisplayName = user.UserName,
                Email = user.Email
            });

            return Ok(new UserOperationResponse
            {
                UserID = new Guid(user.Id),
                ResponseCode = UserOperationResponseCodeEnum.UserUnlocked,
                Message = "User account unlocked"
            });
        }

        [HttpPost]
        [Route("user/{userId}/unlink/{merchantId}")]
        public async Task<IActionResult> Unlink([FromRoute]string userId, [FromRoute]string merchantId)
        {
            var user = userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return NotFound($"User {userId} does not exist");
            }

            var allClaims = await userManager.GetClaimsAsync(user);

            var merchantClaim = allClaims.FirstOrDefault(c => c.Type == Claims.MerchantIDClaim && c.Value == merchantId);

            if (merchantClaim == null)
            {
                return Ok(new UserOperationResponse
                {
                    UserID = new Guid(user.Id),
                    ResponseCode = UserOperationResponseCodeEnum.UserUnlinkedFromMerchant,
                    Message = "User is not linked to merchant"
                });
            }

            await userManager.RemoveClaimAsync(user, merchantClaim);

            // check if user has any merchant claims left, if not we block him
            allClaims = (await userManager.GetClaimsAsync(user)).Where(c => c.Type == Claims.MerchantIDClaim).ToList();

            if (allClaims.Count == 0)
            {
                await userManager.SetLockoutEnabledAsync(user, true);
                await userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));
            }

            return Ok(new UserOperationResponse
            {
                UserID = new Guid(user.Id),
                ResponseCode = UserOperationResponseCodeEnum.UserUnlinkedFromMerchant,
                Message = "User unlinked from merchant"
            });
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
                UserID = new Guid(user.Id),
                Roles = await userManager.GetRolesAsync(user)
            };

            var allClaims = await userManager.GetClaimsAsync(user);

            return Ok(result);
        }
    }
}