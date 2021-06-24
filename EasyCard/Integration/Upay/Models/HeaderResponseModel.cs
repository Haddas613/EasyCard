using System.Runtime.Serialization;

namespace Upay.Models
{
    public class HeaderResponseModel
    {
        /// <summary>
        /// Gets or Sets Success Errorcode
        /// </summary>
        [DataMember(Name = "errorcode")]
        public string Errorcode { get; set; }

        /// <summary>
        /// Gets or Sets Success Errormessage
        /// </summary>
        [DataMember(Name = "errormessage")]
        public string Errormessage { get; set; }

        /// <summary>
        /// Gets or Sets Success errordescription
        /// </summary>
        [DataMember(Name = "errordescription")]
        public string Errordescription { get; set; }

        /// <summary>
        /// Gets or Sets Success Provider
        /// </summary>
        [DataMember(Name = "provider")]
        public string Provider { get; set; }

        /// <summary>
        /// Gets or Sets Success Timestamp
        /// </summary>
        [DataMember(Name = "timestamp")]
        public string Timestamp { get; set; }
    }
}