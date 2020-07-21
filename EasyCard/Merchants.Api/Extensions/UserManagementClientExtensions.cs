using IdentityServerClient;
using Merchants.Shared;
using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedApi = Shared.Api;

namespace Merchants.Api.Extensions
{
    public static class UserManagementClientExtensions
    {
        public static OperationResponse Convert(this UserOperationResponse userOperationResponse, string message = null, string correlationID = null)
        {
            if ((int)userOperationResponse.ResponseCode <= 0)
            {
                return new OperationResponse($"{message ?? Messages.UserOperationFailed}: {userOperationResponse.Message}", userOperationResponse.UserID.ToString(), correlationID, userOperationResponse.ResponseCode.ToString(), userOperationResponse.Message);
            }

            return new OperationResponse($"{message ?? Messages.UserOperationSuccess}: {userOperationResponse.Message}", SharedApi.Models.Enums.StatusEnum.Success, userOperationResponse.UserID.ToString(), correlationID);
        }
    }
}
