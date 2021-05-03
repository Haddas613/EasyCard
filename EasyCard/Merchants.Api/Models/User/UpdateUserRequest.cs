using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class UpdateUserRequest
    {
        public Guid UserID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string FirstName { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public HashSet<string> Roles { get; set; }
    }
}
