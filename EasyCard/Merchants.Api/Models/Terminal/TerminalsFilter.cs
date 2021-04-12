using Merchants.Shared.Enums;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalsFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        public Guid? MerchantID { get; set; }

        public long? TerminalTemplateID { get; set; }

        public string Label { get; set; }

        public DateFilterTypeEnum DateType { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        public TerminalStatusEnum? Status { get; set; }

        public string AggregatorTerminalReference { get; set; }

        public string ProcessorTerminalReference { get; set; }
    }
}
