using Newtonsoft.Json.Linq;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    public class CreateConsumerResponse
    {
        public CreateConsumerResponse()
        {
            Success = true;
        }

        public string ConsumerReference { get; set; }

        public bool Success { get; set; }

        /// <summary>
        /// General error mesage which can be displayed to merchant
        /// </summary>
        public string ErrorMessage { get; set; }

        public JObject ExternalSystemData { get; set; }
    }
}
