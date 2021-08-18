using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer.Data;
using IdentityServer.Helpers;
using IdentityServer.Models;
using IdentityServer.Models.Registration;
using IdentityServer.Services;
using Merchants.Api.Client;
using Merchants.Api.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Security;
using Shared.Helpers.Email;
using Shared.Helpers.Security;

namespace IdentityServer.Controllers
{
    [Route("registration")]
    [SecurityHeaders]
    [AllowAnonymous]
    public class RegistrationController : Controller
    {
        private readonly IMerchantsApiClient merchantsApiClient;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly UserManageService userManageService;
        private readonly UserHelpers userHelpers;
        private readonly CommonLocalizationService localization;
        private readonly UserSecurityService userSecurityService;
        private readonly ILogger logger;

        public RegistrationController(
            IMerchantsApiClient merchantsApiClient,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            UserManageService userManageService,
            UserHelpers userHelpers,
            CommonLocalizationService localization,
            UserSecurityService userSecurityService,
            ILogger<RegistrationController> logger)
        {
            this.merchantsApiClient = merchantsApiClient;
            this.mapper = mapper;
            this.userManager = userManager;
            this.userManageService = userManageService;
            this.userHelpers = userHelpers;
            this.localization = localization;
            this.userSecurityService = userSecurityService;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new RegisterViewModel
            {
                Plans = (await merchantsApiClient.GetPlans()).Data
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            model.Plans = (await merchantsApiClient.GetPlans()).Data;

            if (!ModelState.IsValid)
            {
                return View(nameof(Index), model);
            }

            var emailIsTaken = (await userManager.FindByEmailAsync(model.Email)) != null;

            if (emailIsTaken)
            {
                ModelState.AddModelError(nameof(model.Email), localization.Get("EmailIsAlreadyTaken"));
                return View(nameof(Index), model);
            }

            var merchantRequest = mapper.Map<MerchantRequest>(model);
            var merchantResult = await merchantsApiClient.CreateMerchant(merchantRequest);

            if (merchantResult.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                ModelState.AddModelError("General", merchantResult.Message);
                return View(nameof(Index), model);
            }

            var selectedPlan = model.Plans.First(p => p.PlanID == model.PlanId);
            var terminalRequest = new TerminalRequest
            {
                MerchantID = merchantResult.EntityUID.Value,
                Label = selectedPlan.Title,
                TerminalTemplateID = selectedPlan.TerminalTemplateID
            };
            var terminalResult = await merchantsApiClient.CreateTerminal(terminalRequest);

            if (terminalResult.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                ModelState.AddModelError("General", terminalResult.Message);
                return View(nameof(Index), model);
            }

            var userModel = new IdentityServerClient.CreateUserRequestModel
            {
                Email = model.Email,
                MerchantID = merchantResult.EntityReference,
                Roles = new List<string> { Roles.Merchant, Roles.Manager },
                CellPhone = model.PhoneNumber
            };

            var userResult = await userManageService.CreateUser(userModel);
            if (!userResult.Succeeded)
            {
                ModelState.AddModelError("General", string.Join(",", userResult.Errors.Select(e => e.Description)));
                return View(nameof(Index), model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            //TODO: Confirm email before setting this to true
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);

            //set user password
            var passwordSet = await userSecurityService.TrySetNewPassword(user, model.Password);

            if (passwordSet == false)
            {
                logger.LogError($"{nameof(userSecurityService.TrySetNewPassword)} ERROR. Reverting to force password set");

                //revert to force updating password
                user.PasswordHash = userManager.PasswordHasher.HashPassword(user, model.Password);
                user.PasswordUpdated = DateTime.UtcNow;

                await userManager.UpdateAsync(user);
            }

            var allClaims = await userManager.GetClaimsAsync(user);

            await userManager.AddClaim(allClaims, user, Claims.FirstNameClaim, model.FirstName);
            await userManager.AddClaim(allClaims, user, Claims.LastNameClaim, model.LastName);

            var merchantLinkRequest = new LinkUserToMerchantRequest
            {
                Email = model.Email,
                DisplayName = userHelpers.GetUserFullName(model.FirstName, model.LastName),
                UserID = Guid.Parse(user.Id),
                MerchantID = merchantResult.EntityUID.Value,
                Roles = new List<string>(),
            };

            var merchantLinkResult = await merchantsApiClient.LinkUserToMerchant(merchantLinkRequest);

            if (merchantLinkResult.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                ModelState.AddModelError("General", merchantLinkResult.Message);
                return View(nameof(Index), model);
            }

            return View("RegistrationSuccess");
        }
    }
}