using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
	public class VersioningErrorDetails
	{
		public string ThreeDSServerTransID { get; set; }

		public string ErrorCode { get; set; }

		public string ErrorComponent { get; set; }

		public string ErrorDescription { get; set; }

		public bool PassThrough
        {
            get
            {
				return ErrorCode == "130" || ErrorCode == "303" || ErrorCode == "118";
			}
        }
	}
}
