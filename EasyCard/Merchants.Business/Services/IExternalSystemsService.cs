using Merchants.Business.Entities.Integration;
using Merchants.Business.Models.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merchants.Business.Services
{
    public interface IExternalSystemsService
    {
        ExternalSystem GetExternalSystem(long externalSystemID);

        IEnumerable<ExternalSystem> GetExternalSystems();
    }
}
