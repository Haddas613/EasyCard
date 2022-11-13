using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.Models.Masav
{
    /// <summary>
    /// SetRowsPayed Request
    /// </summary>
    public class MasavFileRowsSetRequest
    {
        [Required]
        public long MasavFileID { get; set; }

        [Required]
        public IEnumerable<long> MasavFileRowIDs { get; set; }
    }
}
