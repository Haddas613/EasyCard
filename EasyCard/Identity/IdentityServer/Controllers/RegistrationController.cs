using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer.Models.Registration;
using Merchants.Api.Client;
using Merchants.Api.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.Api.Security;

namespace IdentityServer.Controllers
{
    [Route("registration")]
    [SecurityHeaders]
    [AllowAnonymous]
    public class RegistrationController : Controller
    {
        private readonly IMerchantsApiClient merchantsApiClient;
        private readonly IMapper mapper;

        public RegistrationController(IMerchantsApiClient merchantsApiClient, IMapper mapper)
        {
            this.merchantsApiClient = merchantsApiClient;
            this.mapper = mapper;
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

            //TODO: 1 create merchant; 2 create user; 3 map merchant to user
            //var merchantRequest = mapper.Map<MerchantRequest>(model);
            //var merchant = await merchantsApiClient.CreateMerchant(merchantRequest);

            return Redirect("/Account/Login");
        }
    }
}