using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models._3DS
{
    public class ErrorVersion
	{
		public string threeDSServerTransID { get; set; }
		public string errorCode { get; set; }
		public string errorComponent { get; set; }
		public string errorDescription { get; set; }
	}
}
