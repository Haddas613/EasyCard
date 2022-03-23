using IdentityServer.Resources;
using Merchants.Api.Client.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Shared.Api.Models.Binding;
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
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string BusinessName { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "NotValidEmailField")]
        [StringLength(100)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [Phone]
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string PhoneNumber { get; set; }
    }
}
