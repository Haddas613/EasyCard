using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class BrowserDetails
    {
        public string BrowserAcceptHeader { get; set; }

        public string BrowserLanguage { get; set; }

        public string BrowserColorDepth { get; set; }

        public string BrowserScreenHeight { get; set; }

        public string BrowserScreenWidth { get; set; }

        public string BrowserTZ { get; set; }

        public string BrowserUserAgent { get; set; }
    }
}
