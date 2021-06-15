using System;
using System.Collections.Generic;
using System.Text;

namespace Upay.Models
{
    public class HeaderModel : HeaderBase
    {
        public string refername { get; set; }
        /// <summary>
        /// 0 for test, 1 for livesystem
        /// </summary>
        public int livesystem { get; set; }
        public string language { get; set; }
    }
}
