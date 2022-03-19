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
using Shared.Helpers.Templating;

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
        private readonly ApplicationSettings applicationSettings;
        private readonly IEmailSender emailSender;

        public RegistrationController(
            IMerchantsApiClient merchantsApiClient,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            UserManageService userManageService,
            UserHelpers userHelpers,
            CommonLocalizationService localization,
            UserSecurityService userSecurityService,
            ILogger<RegistrationController> logger,
            IOptions<ApplicationSettings> applicationSettings,
            IEmailSender emailSender)
        {
            this.merchantsApiClient = merchantsApiClient;
            this.mapper = mapper;
            this.userManager = userManager;
            this.userManageService = userManageService;
            this.userHelpers = userHelpers;
            this.localization = localization;
            this.userSecurityService = userSecurityService;
            this.logger = logger;
            this.applicationSettings = applicationSettings.Value;
            this.emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var model = new RegisterViewModel
            {
                //Plans = (await merchantsApiClient.GetPlans()).Data
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
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

            var emailSubject = Resources.CommonResources.WelcomeToEasyCard;
            var emailTemplateCodeClient = nameof(Register) + "Client";
            var emailTemplateCodeAdmin = nameof(Register) + "Admin";

            var substitutions = new List<TextSubstitution>
            {
                new TextSubstitution(nameof(model.BusinessName), model.BusinessName ),
                new TextSubstitution(nameof(model.FirstName), model.FirstName ),
                new TextSubstitution(nameof(model.LastName), model.LastName ),
                new TextSubstitution(nameof(model.PhoneNumber), model.PhoneNumber ),
                new TextSubstitution(nameof(model.Email), model.Email ),
            };

            foreach (var adminEmail in applicationSettings.SendRegistrationRequestEmailsTo?.Split(",", StringSplitOptions.RemoveEmptyEntries))
            {
                await emailSender.SendEmail(new Email
                {
                    EmailTo = adminEmail,
                    Subject = emailSubject,
                    TemplateCode = emailTemplateCodeAdmin,
                    Substitutions = substitutions.ToArray()
                });
            }

            var email = new Email
            {
                EmailTo = model.Email,
                Subject = emailSubject,
                TemplateCode = emailTemplateCodeClient,
                Substitutions = substitutions.ToArray()
            };

            await emailSender.SendEmail(email);

            return View("RegistrationSuccess");
        }
    }
}