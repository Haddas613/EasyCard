﻿using System;
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
	}
}