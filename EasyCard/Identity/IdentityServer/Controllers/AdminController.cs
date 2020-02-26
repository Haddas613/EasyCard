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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers.Email;
using Shared.Helpers.Security;

namespace IdentityServer.Controllers
{
    [Produces("application/json")]
    [Route("api/admin")]
    [Authorize(AuthenticationSchemes = "token")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly ICryptoService _cryptoService;
        private readonly ApplicationSettings _configuration;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory,
            ApplicationDbContext dataContext,
            ILogger<AdminController> logger,
            IEmailSender emailSender,
            ICryptoService cryptoService,
            IOptions<ApplicationSettings> configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _dataContext = dataContext;
            _emailSender = emailSender;
            _cryptoService = cryptoService;
            _configuration = configuration?.Value;
        }

        [HttpGet]
        [Route("user/{userId}")]
        public async Task<IActionResult> GetUser([FromRoute]string userId)
        {
            var result = await _userManager.FindByEmailAsync(userId);

            return new ObjectResult(result);
        }

        // TODO: validate model
        [HttpPost]
        [Route("user")]
        public async Task<IActionResult> CreateUser([FromBody]CreateUserRequestModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    user = new ApplicationUser { UserName = model.Email, Email = model.Email, /*PhoneNumber = model.CellPhone*/ };
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        _logger.LogInformation("User is not created");
                        // TODO: error details
                        return new BadRequestResult();
                    }

                    _logger.LogInformation("User created a new account");
                }

                //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var code = _cryptoService.EncryptWithExpiration(user.Id, TimeSpan.FromHours(_configuration.ConfirmationEmailExpirationInHours));


                var callbackUrl = Url.EmailConfirmationLink(code, Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                var allClaims = await _userManager.GetClaimsAsync(user);

                //await _userManager.AddClaim(allClaims, user, "extension_PaymentGatewayID", model.PaymentGatewayID.ToString());
                //await _userManager.AddClaim(allClaims, user, "extension_MerchantID", model.MerchantID.ToString());
                //await _userManager.AddClaim(allClaims, user, "extension_FirstName", model.FirstName);
                //await _userManager.AddClaim(allClaims, user, "extension_LastName", model.LastName);

                await _userManager.AddToRoleAsync(user, "Merchant");

                var operationResult = new UserOperationResponse
                {
                    EntityReference = user.Id,
                };

                // TODO: config
                await _userManager.SetTwoFactorEnabledAsync(user, true);

                return new ObjectResult(operationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot process user {model.Email}");
                return StatusCode(500);
            }
        }

        // TODO: validate model
        [HttpPost]
        [Route("terminal")]
        public async Task<IActionResult> AddTerminal([FromBody]CreateUserRequestModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return new ObjectResult("User does not exist") { StatusCode = 404 } ;
                }

                var allClaims = await _userManager.GetClaimsAsync(user);

                //await _userManager.AddClaim(allClaims, user, "extension_MerchantID", model.MerchantID.ToString(), true);

                var operationResult = new UserOperationResponse
                {
                    EntityReference = user.Id,
                };

                // TODO: config
                await _userManager.SetTwoFactorEnabledAsync(user, true);

                return new ObjectResult(operationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot process user {model.Email}");
                return StatusCode(500);
            }
        }

        [HttpDelete]
        [Route("user/{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute]string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required");

            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
                return NotFound($"User {userId} does not exist");

            var operationResult = new UserOperationResponse
            {
                EntityReference = user.Id,
            };

            var result = await _userManager.DeleteAsync(user);
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
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null) return NotFound($"User {userId} does not exist");

            var code = _cryptoService.EncryptWithExpiration(user.Id, TimeSpan.FromHours(_configuration.ResetPasswordEmailExpirationInHours));

            var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, Guid.NewGuid().ToString());

            var result = await _userManager.UpdateAsync(user);
            //if (!result.Succeeded)

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                _logger.LogError($"Unexpected error occurred disabling 2FA for user with ID '{user.Id}'.");
            }

            await _emailSender.SendEmailResetPasswordAsync(user.Email, callbackUrl);

            var operationResult = new UserOperationResponse
            {
                EntityReference = user.Id,
            };

            return new ObjectResult(operationResult);
        }

        [HttpPost]
        [Route("user/{userId}/lock")]
        public async Task<IActionResult> Lock([FromRoute]string userId)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null) return NotFound($"User {userId} does not exist");

            var res = await _userManager.SetLockoutEnabledAsync(user, true);

            if (!res.Succeeded)
            {
                // TODO
                return new JsonResult(new { Status = "error", Errors = res.Errors }) { StatusCode = 500 };
            }

            res = await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));

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
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null) return NotFound($"User {userId} does not exist");

            var res = await _userManager.SetLockoutEnabledAsync(user, true);

            if (!res.Succeeded)
            {
                // TODO
                return new JsonResult(new { Status = "error", Errors = res.Errors }) { StatusCode = 500 };
            }

            res = await _userManager.SetLockoutEndDateAsync(user, null);

            if (!res.Succeeded)
            {
                // TODO
                return new JsonResult(new { Status = "error", Errors = res.Errors }) { StatusCode = 500 };
            }

            // TODO
            return new JsonResult(new { Status = "success" });
        }
    }
}