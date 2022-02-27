using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Configuration
{
    public class EcwidGlobalSettings
    {
        /// <summary>
        /// Received from Ecwid team in email when registering the app 
        /// </summary>
        public string ClientSecret { get; set; }

        public string ApiBaseAddress { get; set; }
    }
}
