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

        public string MDBFilePassword { get; set; }

        public string RapidBaseUrl { get; set; }

        public string RapidAPIKey { get; set; }

        public string RapidItemCategoryName { get; set; }

        public string ECNGAPIKey { get; set; }

        public Environment ECNGEnvironment { get; set; }

        public bool DoNotCreateRapidItems { get; set; }

        public bool DoNotCreateECNGItems { get; set; }

        public bool DoNotCreateBillings { get; set; }
    }
}
