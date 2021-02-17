using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class EnableAuthenticatorViewModel
    {
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}
