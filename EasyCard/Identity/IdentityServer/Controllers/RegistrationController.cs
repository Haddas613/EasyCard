using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer.Data;
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

        public RegistrationController(IMerchantsApiClient merchantsApiClient, IMapper mapper,
            UserManager<ApplicationUser> userManager,
            UserManageService userManageService)
        {
            this.merchantsApiClient = merchantsApiClient;
            this.mapper = mapper;
            this.userManager = userManager;
            this.userManageService = userManageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Index));
            }

            var emailIsTaken = (await userManager.FindByEmailAsync(model.Email)) != null;

            if (emailIsTaken)
            {
                ModelState.AddModelError(nameof(model.Email), "That email is already taken");
                return View(nameof(Index));
            }

            //TODO: 1 create merchant; 2 create user; 3 map merchant to user
            var merchantRequest = mapper.Map<MerchantRequest>(model);
            var merchantResult = await merchantsApiClient.CreateMerchant(merchantRequest);

            if (merchantResult.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                ModelState.AddModelError("General", merchantResult.Message);
                return View(nameof(Index));
            }

            var userModel = new IdentityServerClient.CreateUserRequestModel
            {
                Email = model.Email,
                MerchantID = merchantResult.EntityReference,
                Roles = new List<string>(),
                CellPhone = model.PhoneNumber
            };

            var userResult = await userManageService.CreateUser(userModel);
            if (!userResult.Succeeded)
            {
                ModelState.AddModelError("General", string.Join(",", userResult.Errors.Select(e => e.Description)));
                return View(nameof(Index));
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            //set user password
            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, model.Password);
            await userManager.UpdateAsync(user);

            var merchantLinkRequest = new LinkUserToMerchantRequest
            {
                Email = model.Email,
                DisplayName = model.ContactName,
                UserID = Guid.Parse(user.Id),
                MerchantID = merchantResult.EntityUID.Value,
                Roles = new List<string>(),
            };

            var merchantLinkResult = await merchantsApiClient.LinkUserToMerchant(merchantLinkRequest);

            if (merchantLinkResult.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                ModelState.AddModelError("General", merchantLinkResult.Message);
                return View(nameof(Index));
            }

            return View("RegistrationSuccess");
        }
    }
}