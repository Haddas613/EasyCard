using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopEasyCardConvertorECNG.Models
{
    public class AppConfig
    {
        /// <summary>
        /// Set equal to RapidBaseUrl in case of RapidOne
        /// </summary>
        public string Origin { get; set; }

        public string FullPathToMDBFile { get; set; }

        public string RapidBaseUrl { get; set; }

        public string RapidAPIKey { get; set; }

        public string ECNGAPIKey { get; set; }

        public Environment ECNGEnvironment { get; set; }
    }
}
