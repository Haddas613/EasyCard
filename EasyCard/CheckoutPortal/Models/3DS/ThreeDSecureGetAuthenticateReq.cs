using CheckoutPortal.Models._3DS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models
{
    public class ThreeDSecureGetAuthenticateReq
    {
		public ErrorVersion errorDetails { get;set;}
		public VersioningRequest versioningRequest { get; set; }
		public VersioningResponse versioningResponse { get; set; }
	}
}
