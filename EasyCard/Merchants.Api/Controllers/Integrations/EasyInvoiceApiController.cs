using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyInvoice;
using Merchants.Api.Models.Integrations.EasyInvoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/easy-invoice")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class EasyInvoiceApiController : ApiControllerBase
    {
        private readonly ECInvoiceInvoicing eCInvoicing;

        public EasyInvoiceApiController(ECInvoiceInvoicing eCInvoicing)
        {
            this.eCInvoicing = eCInvoicing;
        }

        [HttpPost]
        [Route("create-customer")]
        public async Task<ActionResult<OperationResponse>> CreateCustomer(CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createUserResult = await eCInvoicing.CreateCustomer(new EasyInvoice.Models.ECCreateCustomerRequest
            {
                Email = request.UserName,
                Password = request.Password,
                TaxID = request.BusinessID
            });

            var response = new OperationResponse(EasyInvoiceMessagesResource.CustomerCreatedSuccessfully, StatusEnum.Success);

            // Currently only possible if 409 (user already exists)
            if (createUserResult.Status != StatusEnum.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = EasyInvoiceMessagesResource.CustomerAlreadyExists;
            }

            return Ok(response);
        }
    }
}