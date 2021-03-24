using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.Sms
{
    public interface ISmsService
    {
        Task<OperationResponse> Send(SmsMessage message);
    }
}
