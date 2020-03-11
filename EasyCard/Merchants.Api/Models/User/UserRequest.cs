using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class UserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
