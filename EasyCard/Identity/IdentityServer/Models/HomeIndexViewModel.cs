using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class HomeIndexViewModel
    {
        public string UserName { get; set; }

        public bool IsAuthorized { get; set; }
    }
}
