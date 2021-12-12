using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions.Enums;

namespace Transactions.Api.Models.Transactions
{
    public class NayaxUpdateRequest
    {
        public NayaxRequest TerminalDetails { get; set; }

        [Required]
        [EnumDataType(typeof(BrandEnum))]
        public BrandEnum Brand { get; set; }

        [Required]
        [EnumDataType(typeof(IssuerAquirEnum))]
        public IssuerAquirEnum Issuer { get; set; }

        [Required]
        [EnumDataType(typeof(IssuerAquirEnum))]
        public IssuerAquirEnum Aquirer { get; set; }

        public bool Success { get; set; }
        public string ResultText { get; set; }
        public int ResultCode { get; set; }

        [Required]
        public string Vuid { get; set; }
        public string Uid { get; set; }
        public string DealNumber { get; set; }
        public string CorrelationID { get; set; }
        public string Issuer_Auth_Num { get; set; }
    }
}
