using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    [Serializable]
    public class GetDocumentTaxReportModel
    {
        [JsonProperty("startDate")]

        public string startDate { get; set; }//, from.ToString("yyyy-MM-dd"));

        [JsonProperty("endDate")]
        public string endDate { get; set; }//to.ToString("yyyy-MM-dd"));


    }
}
