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

		public AuthenticateRequest RequestData { get; set; }

		public AuthenticateResponse ResponseData { get; set; }
		public bool Error()
		{
			return this.errorDetails != null;
		}

		public bool Approve()
		{
			return this.ResponseData?.transStatus?.Equals("Y")??false;
		}
		public bool Reject()
		{
			return this.ResponseData?.transStatus?.Equals("N") ?? false;
		}

		public bool Challenge()
		{
			return this.ResponseData?.transStatus?.Equals("C") ?? false;
		}
	}
}
