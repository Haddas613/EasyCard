using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Merchants.Shared.Models
{
    public class TerminalBankDetails
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int? Bank { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int? BankBranch { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string BankAccount { get; set; }

        [StringLength(8, MinimumLength = 8)]
        public string InstituteNum { get; set; }

        [StringLength(5, MinimumLength = 5)]
        public int InstituteServiceNum { get; set; }

        [StringLength(30)]
        public string InstituteName { get; set; }
    }
}
