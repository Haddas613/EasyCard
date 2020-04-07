using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shva.Models;
using ShvaEMV;

namespace ShvaServiceImitation.Controllers
{
    [Produces("application/xml")]
    [ApiController]
    [Route("")]
    public class MainController : ControllerBase
    {
        private readonly ILogger<MainController> _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }

        public ActionResult Default() => Ok("Ready");

        [HttpPost]
        public async Task<IActionResult> EntryPoint(Envelope request) => 
            request?.Body?.Content switch
             {
                 AshStartRequestBody ashStartRequest => new ObjectResult(await AshStart(ashStartRequest)),
                 AshAuthRequestBody ashAuthRequest => new ObjectResult(await AshAuth(ashAuthRequest)),
                 AshEndRequestBody ashEndRequest => new ObjectResult(await AshEnd(ashEndRequest)),
                 _ => StatusCode(400, "unknown envelope")
             };

        private async Task<AshStartResponseBody> AshStart(AshStartRequestBody body) 
        {
            var response = new AshStartResponseBody();

            response.pinpad = new clsPinPad();
            response.globalObj = new clsGlobal();
            response.AshStartResult = 777;

            return  await Task.FromResult(response);
        }

        private async Task<AshAuthResponseBody> AshAuth(AshAuthRequestBody body)
        {
            var response = new AshAuthResponseBody();

            response.globalObj = new clsGlobal();
            response.pinpad = new clsPinPad();
            response.AshAuthResult = 777;

            return await Task.FromResult(response);
        }

        private async Task<AshEndResponseBody> AshEnd(AshEndRequestBody body)
        {
            var response = new AshEndResponseBody();

            response.pinpad = new clsPinPad();
            response.globalObj = new clsGlobal();
            response.AshEndResult = 777;

            return await Task.FromResult(response);
        }
    }
}
