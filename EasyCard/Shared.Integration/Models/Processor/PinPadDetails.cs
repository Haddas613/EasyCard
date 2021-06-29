using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Integration.Models.Processor
{
    public class PinPadDetails
    {
        [StringLength(64)]
        public string TerminalID { get; set; }
    }
}
