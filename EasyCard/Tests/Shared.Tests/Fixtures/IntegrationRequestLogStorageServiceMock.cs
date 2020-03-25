using Moq;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Tests.Fixtures
{
    public class IntegrationRequestLogStorageServiceMock
    {
        public Mock<IIntegrationRequestLogStorageService> MockObj { get; private set; }

        public IntegrationRequestLogStorageServiceMock()
        {
            MockObj = new Mock<IIntegrationRequestLogStorageService>();
        }
    }
}
