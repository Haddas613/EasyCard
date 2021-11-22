using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Common.Models
{
    public class TokenRequest : CreditCardSecureDetails
    {
        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Consumer ID
        /// </summary>
        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [MaxLength(50)]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// Authorization code
        /// </summary>
        public string OKNumber { get; set; }
    }
}
