using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shared.Integration;
using Shared.Integration.Models;
using Shared.Helpers;

namespace Shared.Tests.CreditCard
{
    public class UrlHelperTests
    {
        [Fact]
        public void UrlHelperBuilUrlTests()
        {
            var url = UrlHelper.BuildUrl("https://3dsc.shva.co.il/3DSInterface/", "api/Versioning");

            Assert.Equal("https://3dsc.shva.co.il:443/3DSInterface/api/Versioning", url);
        }
    }
}
