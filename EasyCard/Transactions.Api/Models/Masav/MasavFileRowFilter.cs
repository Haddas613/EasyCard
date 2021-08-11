using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Masav
{
    public class MasavFileRowFilter : FilterBase
    {
        public long MasavFileID { get; set; }
    }
}
