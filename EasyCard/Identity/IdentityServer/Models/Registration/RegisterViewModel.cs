using Merchants.Api.Client.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.Registration
{
    public class RegisterViewModel
    {
        /// <summary>
        /// Business name
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string BusinessName { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; }

        /// <summary>
        /// Marketing name
        /// </summary>
        [StringLength(50, MinimumLength = 2)]
        public string MarketingName { get; set; }

        [Required]
        [StringLength(9, MinimumLength = 9)]
        public string BusinessID { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [StringLength(32, MinimumLength = 8)]
        public string PasswordRepeat { get; set; }

        [Required]
        public int PlanId { get; set; }

        [BindNever]
        public IEnumerable<PlanSummary> Plans { get; set; }
    }
}
