using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PoalimOnlineBusiness
{
    public class InstitutionSettings
    {
        public int InstituteNumber { get; set; }

        public int SendingInstitute { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string InstitueName { get; set; }
    }
}
