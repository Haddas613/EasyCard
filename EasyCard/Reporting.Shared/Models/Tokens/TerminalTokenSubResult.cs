using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Shared.Models.Tokens
{
    public class TerminalTokenSubResult
    {
        public Guid? TerminalID { get; set; }

        public int Created { get; set; }

        public int Updated { get; set; }

        public int Expired { get; set; }
    }
}
