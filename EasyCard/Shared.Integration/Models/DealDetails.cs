using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class DealDetails
    {
        public string DealDescription { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        public string ConsumerPhone { get; set; }
    }
}
