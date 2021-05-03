using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IdentityServerClient
{
    public class UpdateUserRequestModel
    {
        public string UserID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string FirstName { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}
