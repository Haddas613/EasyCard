using System.Runtime.Serialization;

namespace Upay.Models
{
    public class TransactionsResponseModel
    {
        /// <summary>
        /// Gets or Sets  Results
        /// </summary>
        [DataMember(Name = "results")]
        TransactionResponseModel[] Results = new TransactionResponseModel[1];


    }
}