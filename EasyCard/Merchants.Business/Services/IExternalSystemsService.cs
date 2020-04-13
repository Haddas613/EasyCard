using Merchants.Business.Models.Integration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Services
{
    public interface IExternalSystemsService
    {
        ExternalSystem GetExternalSystem(long externalSystemID);

        IEnumerable<ExternalSystem> GetExternalSystems();
    }
}
