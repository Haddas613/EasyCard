using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Merchants.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Shared.Helpers;
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
        private readonly IExternalSystemsService externalSystemsService;

        public List<Terminal> TerminalsList { get; }

        public Mock<IQueryable<Terminal>> TerminalsListMock { get; }

        public Mock<ITerminalsService> MockObj { get; set; }

        public TerminalsServiceMockSetup(IExternalSystemsService externalSystemsService)
        {
            this.externalSystemsService = externalSystemsService;
            MockObj = new Mock<ITerminalsService>();
            TerminalsList = new List<Terminal>();
            TerminalsListMock = TerminalsList.AsQueryable().BuildMock();
            Setup();
        }

        private void Setup()
        {
            var aggregator = externalSystemsService.ExternalSystems.First(es => es.Type == ExternalSystemTypeEnum.Aggregator);
            var processor = externalSystemsService.ExternalSystems.First(es => es.Type == ExternalSystemTypeEnum.Processor);

            TerminalsList.Add(new Terminal
            {
                TerminalID = Guid.NewGuid().GetSequentialGuid(DateTime.UtcNow),
                MerchantID = Guid.NewGuid().GetSequentialGuid(DateTime.UtcNow),
                Label = "Test 1",
                Status = TerminalStatusEnum.Approved,
                Integrations = new List<TerminalExternalSystem>
                {
                    new TerminalExternalSystem { ExternalSystemID = aggregator.ExternalSystemID, Type = ExternalSystemTypeEnum.Aggregator },
                    new TerminalExternalSystem { ExternalSystemID = processor.ExternalSystemID, Type = ExternalSystemTypeEnum.Processor },
                }
            });

            MockObj.Setup(m => m.GetTerminals())
                .Returns(TerminalsListMock.Object)
                .Verifiable();
        }
    }
}
