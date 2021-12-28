using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class InviteUserRequest
    {
        [Required]
        public Guid MerchantID { get; set; }

        [StringLength(512)]
        public string InviteMessage { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public IEnumerable<Guid> Terminals { get; set; }
    }
}
