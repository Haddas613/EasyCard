using Merchants.Api.Models.Terminal;
using Newtonsoft.Json.Linq;
using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class UserResponse
    {
        public string UserID { get; set; }

        public string DisplayName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public IEnumerable<DictionarySummary<Guid>> Terminals { get; set; }

        public JObject Settings { get; set; }
    }
}
