using Merchants.Business.Models.Integration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Services
{
    public interface IExternalSystemsService
    {
        IReadOnlyList<ExternalSystem> ExternalSystems { get; }

        ExternalSystem GetProcessor(long processorID);

        ExternalSystem GetAggregator(long aggregatorID);
    }
}
