using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Merchants.Api.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var path = Path.Combine(_hostingEnvironment.WebRootPath, "index.html");
            var stream = System.IO.File.OpenRead(path);
            var response = File(stream, "text/html"); // FileStreamResult
            return response;
        }
    }
}