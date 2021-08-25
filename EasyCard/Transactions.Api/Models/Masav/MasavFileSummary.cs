using Newtonsoft.Json;
using Shared.Api.Models;
using Shared.Api.UI;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Masav
{
    public class MasavFileSummary
    {
        public long MasavFileID { get; set; }

        [JsonConverter(typeof(DateFormatConverter), DateFormatType.Date)]
        public DateTime? MasavFileDate { get; set; }

        public DateTime? PayedDate { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? TerminalID { get; set; }

        public string TerminalName { get; set; }

        public decimal? TotalAmount { get; set; }

        public int? InstituteNumber { get; set; }

        public int? SendingInstitute { get; set; }

        public string InstituteName { get; set; }

        [MetadataOptions(Hidden = true)]
        public CurrencyEnum Currency { get; set; }
    }
}
