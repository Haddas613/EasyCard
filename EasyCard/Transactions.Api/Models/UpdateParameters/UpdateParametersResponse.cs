using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.UpdateParameters
{
    public class UpdateParametersResponse
    {
        public Guid? TerminalID { get; set; }

        public UpdateParamsStatusEnum UpdateStatus { get; set; }
    }
}
