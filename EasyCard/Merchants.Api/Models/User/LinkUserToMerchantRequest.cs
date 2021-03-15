using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class LinkUserToMerchantRequest
    {
        [Required]
        public Guid MerchantID { get; set; }

        [Required]
        public Guid UserID { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
