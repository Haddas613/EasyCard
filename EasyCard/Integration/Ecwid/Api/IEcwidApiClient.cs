using Ecwid.Api.Models;
using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ecwid.Api
{
    public interface IEcwidApiClient
    {
        Task<OperationResponse> UpdateOrderStatus(EcwidUpdateOrderStatusRequest request);
    }
}
