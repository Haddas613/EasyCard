using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Shared.Helpers
{
    public class RawRequestResult
    {
        public RawRequestResult()
        {
            this.ResponseHeaders = new NameValueCollection();
        }

        public string ResponseContent { get; set; }

        public NameValueCollection ResponseHeaders { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
