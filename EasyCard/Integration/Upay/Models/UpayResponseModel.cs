using System.Runtime.Serialization;

namespace Upay.Models
{
    public class UpayResponseModel
    {
        /// <summary>
        /// Gets or Sets  Success
        /// </summary>
        [DataMember(Name = "success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or Sets  Header
        /// </summary>
        [DataMember(Name = "header")]
        public HeaderResponseModel Header { get; set; }

        /// <summary>
        /// Gets or Sets  Result
        /// </summary>
        [DataMember(Name = "result")]
        public ResultResponseModel Result { get; set; }
    }
}