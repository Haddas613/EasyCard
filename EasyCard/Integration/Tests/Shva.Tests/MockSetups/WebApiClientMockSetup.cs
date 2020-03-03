using Moq;
using Shared.Helpers;
using Shva.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shva.Tests.MockSetups
{
    public class WebApiClientMockSetup
    {
        public Mock<IWebApiClient> MockObj { get; private set; }
        public WebApiClientMockSetup()
        {
            MockObj = new Mock<IWebApiClient>();
        }

        private void Setup()
        {
            //TODO: Mock IWebApiClient
            //This example can be used as reference
            MockObj.Setup(o => o.PostXml<Envelope>(
                It.IsAny<string>(), 
                "/Service/Service.asmx", 
                It.Is<Envelope>(obj => obj.Body != null), //You can add any logic to check whether Passed argument will invoke this particular Mock
                null, null, null))
                .Returns<Task<Envelope>>(r => Task.FromResult(new Envelope()));

        }
    }
}
