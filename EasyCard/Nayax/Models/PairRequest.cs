using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Nayax.Models
{
    public class PairRequest
    {
        [Required]
        public Guid? ECTerminalID { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string posName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string terminalID { get; set; }

        public string CorrelationId { get; set; }

        public PairRequest(string posName, string terminalID)
        {
            this.terminalID = terminalID;
            this.posName = posName;
        }
    }
}
