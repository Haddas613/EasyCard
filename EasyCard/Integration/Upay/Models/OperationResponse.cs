using Newtonsoft.Json.Converters;
using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Upay.Models
{
    /// <summary>
    /// Common operation results information
    /// </summary>
    [DataContract]
    public partial class OperationResponse
    {
        public OperationResponse()
        {
            this.Errors = new List<Error>();
        }

        /// <summary>
        /// Gets or Sets Errors
        /// </summary>
        [DataMember(Name = "errors")]
        public List<Error> Errors { get; set; }
        /// <summary>
        /// Gets or Sets Cashierid
        /// </summary>
        [DataMember(Name = "cashierid")]
        public long Cashierid { get; set; }

        /// <summary>
        /// Gets or Sets TotalAmount
        /// </summary>
        [DataMember(Name = "totalamount")]
        public string TotalAmount { get; set; }

        /// <summary>
        /// Gets or Sets CreditcardCompanycode
        /// </summary>
        [DataMember(Name = "creditcardcompanycode")]
        public string CreditcardCompanycode { get; set; }

        /// <summary>
        /// Gets or Sets MerchantNumber
        /// </summary>
        [DataMember(Name = "merchantnumber")]
        public string MerchantNumber { get; set; }

        /// <summary>
        /// Gets or Sets SessionId
        /// </summary>
        [DataMember(Name = "sessionid")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or Sets ErrorMessage
        /// </summary>
        [DataMember(Name = "errormessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or Sets WebUrl
        /// </summary>
        [DataMember(Name = "weburl")]
        public string WebUrl { get; set; }

        /// <summary>
        /// Gets or Sets ErrorDescription
        /// </summary>
        [DataMember(Name = "errordescription")]
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Operation Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusEnum? Status { get; set; }

    }
}
