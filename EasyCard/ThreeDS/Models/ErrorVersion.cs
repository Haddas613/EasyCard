using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
	public class ErrorVersion
	{
		public string threeDSServerTransID { get; set; }
		public string errorCode { get; set; }
		public string errorComponent { get; set; }
		public string errorDescription { get; set; }
	}
}
