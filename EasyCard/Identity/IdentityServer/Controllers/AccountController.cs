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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers.Email;
using Shared.Helpers.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
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
            IAuditLogger auditLogger)
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
                    //_logger.LogWarning("User account locked out.");

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

            //var result = await _userManager.ConfirmEmailAsync(user, code);
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

            //var user = await _userManager.FindByIdAsync(model.UserId);
            //if (user == null)
            //{
            //    _logger.LogError($"UserId does not exist");
            //    //throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            //    return RedirectToAction(nameof(HomeController.Index), "Home");
            //}
            //var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, model.Code);

            //if (!confirmEmailResult.Succeeded)
            //{
            //    var errors = string.Join(", ", confirmEmailResult.Errors.Select(err => err.Code + ":" + err.Description));
            //    _logger.LogError($"Email confirmation failed: {errors}");
            //    return RedirectToAction(nameof(HomeController.Index), "Home");
            //}

            //return View(result.Succeeded ? "ConfirmEmail" : "Error");

            // var model = new ConfirmEmailViewModel { Code = code };
            //return View(model);

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
                await auditLogger.RegisterConfirmEmail(user);
                return RedirectToAction(nameof(HomeController.Index), "Home");

                //return RedirectToAction(nameof(ManageController.EnableAuthenticator), "Manage");
            }

            var pwderrors = string.Join(", ", addPasswordResult.Errors.Select(err => err.Code + ":" + err.Description));
            logger.LogError($"User {user.Email} set password failed: {pwderrors}");

            AddErrors(addPasswordResult);
            return View();
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
                    logger.LogError($"User {model.Email} does not exist");

                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation), new { Message = IdentityMessages.ForgotPasswordInvalidEmail });
                }

                //Validate user bank account
                if (configuration.ForgotPasswordCheckBankAccountNumber)
                {
                    // TODO:

                    //var validationResult = await _managementApiClient.ValidateUser(model.Email, model.BankAccount);
                    //if (validationResult?.Status == StatusEnum.Error)
                    //{
                    //    _logger.LogError($"User email '{model.Email}' and bank account number '{model.BankAccount}' mismatched or user is not valid");
                    //    // Don't reveal that the user does not exist or is not confirmed
                    //    return RedirectToAction(nameof(ForgotPasswordConfirmation), new { Message = validationResult.Message });
                    //}
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                //var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                var code = cryptoService.EncryptWithExpiration(user.Id, TimeSpan.FromHours(configuration.ResetPasswordEmailExpirationInHours));

                var callbackUrl = Url.ResetPasswordCallbackLink(code, Request.Scheme);

                var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
                if (!disable2faResult.Succeeded)
                {
                    logger.LogError($"Unexpected error occurred disabling 2FA for user with ID '{user.Id}'.");
                }

                await emailSender.SendEmailResetPasswordAsync(model.Email, callbackUrl);

                await auditLogger.RegisterForgotPassword(model.Email);
                return RedirectToAction(nameof(ForgotPasswordConfirmation), new { Message = IdentityMessages.ForgotPasswordPasswordReseted });
            }

            // If we got this far, something failed, redisplay form
            model.CheckBankAccount = configuration.ForgotPasswordCheckBankAccountNumber;
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation(string message)
        {
            ViewData["ValidationMessage"] = message;
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

            //var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
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

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

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
            if (context?.IdP != null && await schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
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
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
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
    }
}