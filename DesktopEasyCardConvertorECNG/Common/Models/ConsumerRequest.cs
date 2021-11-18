using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class ConsumerRequest
    {
        /// <summary>
        /// Target terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer name
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 3)]
        public string ConsumerName { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string ConsumerPhone2 { get; set; }

        /// <summary>
        /// End-customer National ID
        /// </summary>
        [StringLength(50)]
        public string ConsumerNationalID { get; set; }

        /// <summary>
        /// End-customer note details
        /// </summary>
        [StringLength(256)]
        public string Note { get; set; }

        /// <summary>
        /// End-customer address
        /// </summary>
        public Address ConsumerAddress { get; set; }

        /// <summary>
        /// ID in external system
        /// </summary>
        [StringLength(50)]
        public string ExternalReference { get; set; }

        /// <summary>
        /// ID in BillingDesktop system
        /// </summary>
        [StringLength(50)]

        public string BillingDesktopRefNumber { get; set; }

        /// <summary>
        /// Origin of customer
        /// </summary>
        [StringLength(50)]
        public string Origin { get; set; }

        public bool Active { get; set; } = true;

        public string ConsumerSecondPhone { get; set; }

        public string ConsumerNote { get; set; }

    }
}
