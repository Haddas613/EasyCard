using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MerchantProfileApi.Models.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Models;

namespace MerchantProfileApi.Controllers.External
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        public async Task<ActionResult<OperationResponse>> TransactionStatus(TransactionsStatusRequest request)
        {
            return Ok();
        }
    }
}