using Merchants.Business.Entities.Integration;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Transactions.Tests.MockSetups
{
    public class TerminalsServiceMockSetup
    {
        public List<Terminal> TerminalsList { get; }

        public Mock<IQueryable<Terminal>> TerminalsListMock { get; }

        public Mock<ITerminalsService> MockObj { get; set; }

        public TerminalsServiceMockSetup(bool useDefaultSetup = true)
        {
            MockObj = new Mock<ITerminalsService>();
            TerminalsList = new List<Terminal>();
            TerminalsListMock = TerminalsList.AsQueryable().BuildMock();

            if (useDefaultSetup)
            {
                Setup();
            }
        }

        private void Setup()
        {
            TerminalsList.Add(new Terminal
            {
                TerminalID = 1,
                MerchantID = 1,
                Label = "Test 1",
                Status = Merchants.Shared.Enums.TerminalStatusEnum.Approved,
                Integrations = new List<TerminalExternalSystem>
                {
                    new TerminalExternalSystem { ExternalSystem = new ExternalSystem { Type = Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator, Name = "TestAggregator" } },
                    new TerminalExternalSystem { ExternalSystem = new ExternalSystem { Type = Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor, Name = "TestProcessor" } },
                }
            });

            MockObj.Setup(m => m.GetTerminals())
                .Returns(TerminalsListMock.Object)
                .Verifiable();
        }
    }
}
