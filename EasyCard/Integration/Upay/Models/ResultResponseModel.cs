using System.Runtime.Serialization;

namespace Upay.Models
{
    public class ResultResponseModel
    {
        /// <summary>
        /// Gets or Sets  Sessionid
        /// </summary>
        [DataMember(Name = "sessionid")]
        public string Sessionid { get; set; }

        /// <summary>
        /// Gets or Sets  Transactions
        /// </summary>
        [DataMember(Name = "transactions")]
        public TransactionResponseModel[] Transactions { get; set; }

    }
}