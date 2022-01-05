using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    [Serializable]
    public class GetDocumentReportModel
    {
        [JsonProperty("startDate")]

        public string StartDate {get;set;}//, from.ToString("yyyy-MM-dd"));

        [JsonProperty("endDate")]
        public string EndDate { get; set; }//to.ToString("yyyy-MM-dd"));

        [JsonProperty("onlyCancelled")]
        public bool OnlyCancelled { get; set; }

        [JsonProperty("includeCancelled")]
        public bool IncludeCancelled { get; set; }

    }
}
