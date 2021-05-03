using System;
using System.Collections.Generic;
using System.Text;

namespace ClearingHouse.Models
{
    public class GetMerchantsQuery
    {
        /// <summary>
        /// KYC approval status
        /// </summary>
        public List<string> Status { get; set; }

        // TODO: do we need this (?)
        public long? MerchantID { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string MerchantReference { get; set; }

        /// <summary>
        /// Risk rate
        /// </summary>
        public List<int> RiskRate { get; set; }

        /// <summary>
        /// Business sector
        /// </summary>
        public string BusinessArea { get; set; }

        /// <summary>
        /// Merchant business namer or merchant first name or last name
        /// </summary>
        public string MerchantName { get; set; }

        /// <summary>
        /// Business registration Id
        /// </summary>
        public string BusinessId { get; set; }

        /// <summary>
        /// Cell phone or business phone
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Show merchants with locked accounts
        /// </summary>
        public bool? IsAccountLocked { get; set; }

        /// <summary>
        /// Payment gateway Terminal Reference
        /// </summary>
        public string TerminalReference { get; set; }

        /// <summary>
        /// Merchant's contact person national ID number
        /// </summary>
        public string NationalIdNumber { get; set; }

        ///// <summary>
        ///// Clearing Company
        ///// </summary>
        //public long? ClearingCompany { get; set; }

        /// <summary>
        /// Visa reference
        /// </summary>
        public string ClearingCompanyReference { get; set; }

        /// <summary>
        /// Show merchants with NikionEnabled
        /// </summary>
        public bool? NikionEnabled { get; set; }

        public string GlobalSearch { get; set; }

        public string IsracardID { get; set; }

        public bool? RavMotav { get; set; }

        public bool? IsDisable { get; set; }
    }
}
