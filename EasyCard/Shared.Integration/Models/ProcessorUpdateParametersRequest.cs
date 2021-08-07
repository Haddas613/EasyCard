using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorUpdateParametersRequest
    {
        /// <summary>
        /// Shva terminal settings
        /// </summary>
        public object ProcessorSettings { get; set; }

        /// <summary>
        /// Easy Card TerminalID
        /// </summary>
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }
    }
}
