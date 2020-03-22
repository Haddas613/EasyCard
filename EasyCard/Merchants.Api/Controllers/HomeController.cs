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
        private readonly IWebHostEnvironment hostingEnvironment;

        public HomeController(IWebHostEnvironment environment)
        {
            hostingEnvironment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var path = Path.Combine(hostingEnvironment.WebRootPath, "index.html");
            var stream = System.IO.File.OpenRead(path);
            var response = File(stream, "text/html"); // FileStreamResult
            return response;
        }
    }
}