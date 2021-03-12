using IdentityModel;
using IdentityServer.Helpers;
using IdentityServer.Messages;
using IdentityServer.Models;
using IdentityServer.Security;
using IdentityServer.Security.Auditing;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Merchants.Api.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Api.Security;
using Shared.Helpers.Email;
using Shared.Helpers.Security;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private const string TwoFactorAuthProvider = "Phone";

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IIdentityServerInteractionService interaction;
        private readonly IClientStore clientStore;
        private readonly IAuthenticationSchemeProvider schemeProvider;
        private readonly IEventService events;

        private readonly IEmailSender emailSender;
        private readonly ILogger logger;

        private readonly ICryptoService cryptoService;

        private readonly ApplicationSettings configuration;

        private readonly IAuditLogger auditLogger;
        private readonly AzureADSettings azureADConfig;
        private readonly ApiSettings apiConfiguration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            ICryptoService cryptoService,
            IOptions<ApplicationSettings> configuration,
            IAuditLogger auditLogger,
            IOptions<AzureADSettings> azureADConfig,
            IOptions<ApiSettings> apiConfiguration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.interaction = interaction;
            this.clientStore = clientStore;
            this.schemeProvider = schemeProvider;
            this.events = events;

            this.emailSender = emailSender;
            this.logger = logger;
            this.cryptoService = cryptoService;
            this.configuration = configuration.Value;
            this.auditLogger = auditLogger;

            this.azureADConfig = azureADConfig.Value;
            this.apiConfiguration = apiConfiguration.Value;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await interaction.GrantConsentAsync(context, new ConsentResponse() { Error = AuthorizationError.AccessDenied });

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (await clientStore.IsPkceClientAsync(context.Client.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await userManager.FindByNameAsync(model.Username);
                    await events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client?.ClientId));
                    await auditLogger.RegisterLogin(user);

                    if (context != null)
                    {
                        if (await clientStore.IsPkceClientAsync(context.Client?.ClientId))
                        {
                            // if the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(model.ReturnUrl);
                    }

                    // request for a local page
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");
                    }
                }

                if (result.IsLockedOut)
                {
                    //logger.LogWarning("User account locked out.");

                    //await _managementApiClient.RegisterLocked(model.Email);

                    return RedirectToAction(nameof(Lockout));
                }

                await events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client?.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await signInManager.SignOutAsync();

                // raise the logout event
                await events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
                var user = await userManager.FindByIdAsync(User.GetSubjectId());
                await auditLogger.RegisterLogout(user);
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string code)
        {
            if (code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var userId = cryptoService.DecryptWithExpiration(code);

            if (userId == null)
            {
                logger.LogError($"Confirmation code expired or invalid");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogError($"Confirmation code is invalid. User {userId} does not exist");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            //var result = await userManager.ConfirmEmailAsync(user, code);
            //return View(result.Succeeded ? "ConfirmEmail" : "Error");

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                logger.LogError($"User {user.Email} already has password");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var model = new ConfirmEmailViewModel { Code = code };
            return View(model);
        }

        //TODO: validate model state
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model)
        {
            if (model.Code == null)
            {
                logger.LogError($"Confirmation code not specified");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var userId = cryptoService.DecryptWithExpiration(model.Code);

            // TODO: show error message
            if (userId == null)
            {
                logger.LogError($"Confirmation code expired or invalid");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogError($"Confirmation code is invalid. User {userId} does not exist");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var addPasswordResult = await userManager.AddPasswordAsync(user, model.Password);
            if (addPasswordResult.Succeeded)
            {
                await auditLogger.RegisterConfirmEmail(user, $"{model.FirstName} {model.LastName}".Trim());

                var allClaims = await userManager.GetClaimsAsync(user);

                await userManager.AddClaim(allClaims, user, Claims.FirstNameClaim, model.FirstName);
                await userManager.AddClaim(allClaims, user, Claims.LastNameClaim, model.LastName);

                if (await userManager.IsInRoleAsync(user, Roles.Merchant))
                {
                    return Redirect(apiConfiguration.MerchantProfileURL);
                }
                else
                {
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                //return RedirectToAction(nameof(ManageController.EnableAuthenticator), "Manage");
            }
            else
            {
                var pwderrors = string.Join(", ", addPasswordResult.Errors.Select(err => err.Code + ":" + err.Description));
                logger.LogError($"User {user.Email} set password failed: {pwderrors}");

                AddErrors(addPasswordResult);
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            ViewData["CompanyName"] = configuration.CompanyName;
            var model = new ForgotPasswordViewModel
            {
                CheckBankAccount = configuration.ForgotPasswordCheckBankAccountNumber
            };
            return View(model);
        }

        // TODO: use custom certificate
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!model.CheckBankAccount)
            {
                _ = ModelState.Remove(nameof(model.BankAccount));
            }

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    logger.LogError($"ForgotPassword attempt to reset non existing user with email: {model.Email}");

                    //Don't reveail if user doesn't exists
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                //Validate user bank account
                if (configuration.ForgotPasswordCheckBankAccountNumber)
                {
                    // TODO:

                    //var validationResult = await _managementApiClient.ValidateUser(model.Email, model.BankAccount);
                    //if (validationResult?.Status == StatusEnum.Error)
                    //{
                    //    logger.LogError($"User email '{model.Email}' and bank account number '{model.BankAccount}' mismatched or user is not valid");
                    //    // Don't reveal that the user does not exist or is not confirmed
                    //    return RedirectToAction(nameof(ForgotPasswordConfirmation), new { Message = validationResult.Message });
                    //}
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                //var code = await userManager.GeneratePasswordResetTokenAsync(user);

                var code = cryptoService.EncryptWithExpiration(user.Id, TimeSpan.FromHours(configuration.ResetPasswordEmailExpirationInHours));

                var callbackUrl = Url.ResetPasswordCallbackLink(code, Request.Scheme);

                var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
                if (!disable2faResult.Succeeded)
                {
                    logger.LogError($"Unexpected error occurred disabling 2FA for user with ID '{user.Id}'.");
                }

                await emailSender.SendEmailResetPasswordAsync(model.Email, callbackUrl);

                await auditLogger.RegisterForgotPassword(model.Email);
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            model.CheckBankAccount = configuration.ForgotPasswordCheckBankAccountNumber;
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewData["Message"] = IdentityMessages.ForgotPasswordSuccess;
            ViewData["PreviousUrl"] = Request.Headers["Referer"].ToString();
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string code = null)
        {
            if (code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var userId = cryptoService.DecryptWithExpiration(code);

            if (userId == null)
            {
                logger.LogError($"Confirmation code expired or invalid");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogError($"Confirmation code is invalid");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            //var result = await userManager.ResetPasswordAsync(user, model.Code, model.Password);
            //if (result.Succeeded)
            //{
            //    return RedirectToAction(nameof(ResetPasswordConfirmation));
            //}

            if (model.Code == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var userId = cryptoService.DecryptWithExpiration(model.Code);

            if (userId == null)
            {
                logger.LogError($"Confirmation code expired or invalid");
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            if (userId != user.Id)
            {
                logger.LogError($"Invalid email address provided: userId = {userId} and email = {model.Email}");
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, model.Password);

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                //throw exception......
            }
            else
            {
                await auditLogger.RegisterResetPassword(user);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // TODO: log errors
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError("ExternalLogin", $"Error from external provider: {remoteError}");
                logger.LogWarning($"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var tempUser = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            if (tempUser?.Succeeded != true)
            {
                //throw new Exception("External authentication error");
                logger.LogWarning($"External authentication error: {tempUser?.Failure?.Message}");
                return RedirectToAction(nameof(Login));
            }

            // retrieve claims of the external user
            var externalUser = tempUser.Principal;

            if (externalUser == null)
            {
                throw new Exception("External authentication error");
            }

            // retrieve claims of the external user
            var claims = externalUser.Claims.ToList();

            // try to determine the unique id of the external user - the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);

            if (userIdClaim == null)
            {
                userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            }

            foreach (var claim in claims)
            {
                logger.LogWarning(claim.Type + ": " + claim.Value);
            }

            if (userIdClaim == null)
            {
                //throw new Exception("Unknown userid");
                logger.LogWarning("Unknown userid");
                return RedirectToAction(nameof(Login));
            }

            var externalUserId = userIdClaim.Value;
            var externalProvider = userIdClaim.Issuer;
            var email = (tempUser.Principal.FindFirstValue(ClaimTypes.Email) ?? tempUser.Principal.FindFirstValue(JwtClaimTypes.Email)) ?? tempUser.Principal.FindFirstValue(ClaimTypes.Name);

            if (email == null)
            {
                //throw new Exception("Unknown email"); --add details to log
                logger.LogWarning("Unknown email");
                return RedirectToAction(nameof(Login));
            }

            var firstName = tempUser.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = tempUser.Principal.FindFirstValue(ClaimTypes.Surname);

            var justName = tempUser.Principal.FindFirstValue("name");

            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName)) firstName = justName;

            // TODO: get roles
            var roles = claims.FindAll(x => x.Type == "groups");

            var isECNGBillingAdmin = roles.FirstOrDefault(x => x.Value == azureADConfig.AzureAdBillingAdministratorGrpId) != null; //"ECNGBillingAdmin"
            var isECNGBusinessAdmin = roles.FirstOrDefault(x => x.Value == azureADConfig.AzureAdBusinessAdministratorGrpId) != null; //"ECNGBusinessAdmin"

            // Sign in the user with this external login provider if the user already has a login.
            var result = await signInManager.ExternalLoginSignInAsync(externalProvider, externalUserId, true, true); //ispersistent:false
            if (result.Succeeded) // external user already in identity database
            {
                var user = await userManager.FindByEmailAsync(email);

                var allClaims = await userManager.GetClaimsAsync(user);

                await userManager.AddClaim(allClaims, user, Claims.FirstNameClaim, firstName);
                await userManager.AddClaim(allClaims, user, Claims.LastNameClaim, lastName);

                logger.LogInformation("User logged in with {Name} provider.", "test");

                await RefreshExternalUserRoles(user, isECNGBillingAdmin, isECNGBusinessAdmin);

                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }

            // If the user does not have an account, then create an account.

            var newUser = await userManager.FindByEmailAsync(email);
            if (newUser == null)
            {
                newUser = new ApplicationUser { UserName = email, Email = email };
                var newUserResult = await userManager.CreateAsync(newUser);
                if (!newUserResult.Succeeded)
                {
                    logger.LogInformation("User is not created");

                    // TODO: error details
                    return new BadRequestResult();
                }
            }

            var allClaimsNew = await userManager.GetClaimsAsync(newUser);

            await userManager.AddClaim(allClaimsNew, newUser, Claims.FirstNameClaim, firstName);
            await userManager.AddClaim(allClaimsNew, newUser, Claims.LastNameClaim, lastName);

            await RefreshExternalUserRoles(newUser, isECNGBillingAdmin, isECNGBusinessAdmin);

            var userLoginInfo = new UserLoginInfo(externalProvider, externalUserId, email);

            var addLoginresult = await userManager.AddLoginAsync(newUser, userLoginInfo);
            if (!addLoginresult.Succeeded)
            {
                logger.LogInformation("User is not created");

                // TODO: error details
                return new BadRequestResult();
            }

            await signInManager.SignInAsync(newUser, isPersistent: true); //isPersistent: false
            logger.LogInformation("User created an account using {Name} provider.", userLoginInfo.LoginProvider);
            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            if (user.TwoFactorEnabled)
            {
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            var model = new EnableAuthenticatorViewModel
            {
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(string submit, EnableAuthenticatorViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            if (user.TwoFactorEnabled)
            {
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var setPhoneResult = await userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            var code = await userManager.GenerateTwoFactorTokenAsync(user, TwoFactorAuthProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            if (submit == "sms" && !string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                throw new NotImplementedException();

                // TODO: implement SMS

                //var message = configuration.UserEnableTwoFactorSmsTemplate.Replace("{code}", code);

                //var messageId = Guid.NewGuid().ToString();
                //var phoneNumber = await userManager.GetPhoneNumberAsync(user);

                //var response = await this.smsService.Send(new ClearingHouse.Shared.Services.SmsMessage
                //{
                //    MerchantID = null,
                //    MessageId = messageId,
                //    Body = message,
                //    From = configuration.SmsFromDetails,
                //    To = phoneNumber
                //});

                //if (response.Status == StatusEnum.Error)
                //{
                //    return View("Error");
                //}
            }
            else
            {
                await this.emailSender.Send2faEmailAsync(user.Email, code);
            }

            return RedirectToAction(nameof(VerifyAuthentificatorCode));
        }

        [HttpGet]
        public async Task<IActionResult> VerifyAuthentificatorCode()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            if (user.TwoFactorEnabled)
            {
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            return View(new VerifyAuthentificatorCodeViewModel { PhoneNumber = user.PhoneNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthentificatorCode(VerifyAuthentificatorCodeViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            if (user.TwoFactorEnabled)
            {
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            if (!ModelState.IsValid)
            {
                model.PhoneNumber = user.PhoneNumber;
                return View(model);
            }

            //Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await userManager.VerifyTwoFactorTokenAsync(user, TwoFactorAuthProvider, verificationCode);

            if (!is2faTokenValid)
            {
                model.PhoneNumber = user.PhoneNumber;
                ModelState.AddModelError("Code", "Verification code is invalid.");
                return View(model);
            }

            await userManager.SetTwoFactorEnabledAsync(user, true);
            await userManager.ResetAuthenticatorKeyAsync(user);
            logger.LogInformation($"User with ID {user.Id} has confirmed 2FA with mobile phone number {user.PhoneNumber}", user.Id);

            var allClaims = await userManager.GetClaimsAsync(user);
            await auditLogger.RegisterTwoFactorCompleted(user);

            // TODO: from query string
            return Redirect(apiConfiguration.MerchantProfileURL);
        }

        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = user.TwoFactorEnabled,
                RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user),
            };

            return View(model);
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
                return Redirect(returnUrl);
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await interaction.GetAuthorizationContextAsync(returnUrl);
            var isAdmin = (User?.IsAdmin()).Value;

            if (context?.IdP != null && await schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    IsAuthorized = User?.Identity.IsAuthenticated == true,
                    UserName = User.GetDoneByName(),
                    IsAdmin = isAdmin,
                    ClientSystemURL = isAdmin ? apiConfiguration.MerchantsManagementApiAddress : apiConfiguration.MerchantProfileURL
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase)
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client?.ClientId != null)
            {
                var client = await clientStore.FindEnabledClientByIdAsync(context.Client?.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray(),
                IsAuthorized = User?.Identity.IsAuthenticated == true,
                UserName = User.GetDoneByName(),
                IsAdmin = isAdmin,
                ClientSystemURL = isAdmin ? apiConfiguration.MerchantsManagementApiAddress : apiConfiguration.MerchantProfileURL
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private async Task RefreshExternalUserRoles(ApplicationUser newUser, bool isECNGBillingAdmin, bool isECNGBusinessAdmin)
        {
            if (User.IsInRole(Roles.BusinessAdministrator) && !isECNGBusinessAdmin)
            {
                logger.LogWarning($"Remove user {newUser.Email} from role BusinessAdministrator");
                await userManager.RemoveFromRoleAsync(newUser, Roles.BusinessAdministrator);
            }
            else if (!User.IsInRole(Roles.BusinessAdministrator) && isECNGBusinessAdmin)
            {
                await userManager.AddToRoleAsync(newUser, Roles.BusinessAdministrator);
            }

            if (User.IsInRole(Roles.BillingAdministrator) && !isECNGBillingAdmin)
            {
                logger.LogWarning($"Remove user {newUser.Email} from role BillingAdministrator");
                await userManager.RemoveFromRoleAsync(newUser, Roles.BillingAdministrator);
            }
            else if (!User.IsInRole(Roles.BillingAdministrator) && isECNGBillingAdmin)
            {
                await userManager.AddToRoleAsync(newUser, Roles.BillingAdministrator);
            }
        }
    }
}