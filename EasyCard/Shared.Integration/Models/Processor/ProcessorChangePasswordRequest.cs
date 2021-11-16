using Newtonsoft.Json.Linq;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorChangePasswordRequest
    {
        public ProcessorChangePasswordRequest()
        {
        }

        /// <summary>
        /// Shva terminal settings
        /// </summary>
        public object ProcessorSettings { get; set; }

        public string NewPassword { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Easy Card Terminal ID
        /// </summary>
        public Guid TerminalID { get; set; }
    }
}
