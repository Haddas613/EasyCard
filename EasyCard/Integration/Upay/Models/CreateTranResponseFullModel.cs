using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    public class CreateTranResponseFullModel
    {
        /// <summary>
        /// Gets or Sets  Results
        /// </summary>
        [DataMember(Name = "results")]
        public UpayResponseModel[] Results = new UpayResponseModel[2];
    }
}
