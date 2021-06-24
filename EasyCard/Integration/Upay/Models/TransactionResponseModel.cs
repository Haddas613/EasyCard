using System.Runtime.Serialization;

namespace Upay.Models
{
    public class TransactionResponseModel
    {
        /// <summary>
        /// Gets or Sets  Cashierid
        /// </summary>
        [DataMember(Name = "cashierid")]
        public long Cashierid { get; set; }

        /// <summary>
        /// Gets or Sets  Totalamount
        /// </summary>
        [DataMember(Name = "totalamount")]
        public string Totalamount { get; set; }

        /// <summary>
        /// Gets or Sets  Merchantnumber
        /// </summary>
        [DataMember(Name = "merchantnumber")]
        public string Merchantnumber { get; set; }

        /// <summary>
        /// Gets or Sets Terminal
        /// </summary>
        [DataMember(Name = "terminal")]
        public string Terminal { get; set; }

    }
}