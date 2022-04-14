using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Merchants.Shared.Models
{
    public class TerminalBankDetails
    {
        [StringLength(9, MinimumLength = 3)]
        public string InstituteNum { get; set; }

        public int? InstituteServiceNum { get; set; }

        [StringLength(30)]
        public string InstituteName { get; set; }
    }
}
