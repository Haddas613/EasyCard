using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nayax;
using Nayax.Models;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/nayax")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class NayaxApiController : ApiControllerBase
    {
        private readonly Nayax.NayaxProcessor nayaxProcessor;

        public NayaxApiController(NayaxProcessor nayaxProcessor)
        {
            this.nayaxProcessor = nayaxProcessor;
        }

        [HttpPost]
        public async Task<ActionResult<OperationResponse>> PairDevice(PairRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //get settings
            var pairResult = await nayaxProcessor.PairDevice(request);


            var response = new OperationResponse("Paired Successfully"/*NayaxMessagesResource.PairedSuccessfully*/, StatusEnum.Success);//TODO
                                                                                                                                        //
                                                                                                                                        //  }

            return response;// Ok(response);TODO!!!!!
        }

    }
}
