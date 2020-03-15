using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Tokens
{
    public class TokenRequest
    {
        /// <summary>
        /// cvv presense should be validated according business settings
        /// </summary>
        //[Required]
        [StringLength(4, MinimumLength = 3)]
        public string Cvv { get; set; }

        /// <summary>
        /// Visa and Visa Electron: 13 or 16
        /// Mastercard: 16
        /// Discover: 16
        /// American Express: 15
        /// Diner's Club (including enRoute, International, Blanche): 14
        /// Maestro: 12 to 19 (multi-national Debit Card)
        /// Laser: 16 to 19 (Ireland Debit Card)
        /// Switch: 16, 18 or 19 (United Kingdom Debit Card)
        /// Solo: 16, 18 or 19 (United Kingdom Debit Card)
        /// JCB: 15 or 16 (Japan Credit Bureau)
        /// China UnionPay: 16 (the only domestic bank card organization in the People's Republic of China)
        /// </summary>
        [Required]
        [StringLength(19, MinimumLength = 10)] // TODO: change rules?
        public string CardNumber { get; set; }

        public CardExpiration CardExpiration { get; set; }
    }
}
