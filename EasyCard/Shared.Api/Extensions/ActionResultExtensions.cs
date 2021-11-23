using Microsoft.AspNetCore.Mvc;
using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Extensions
{
    public static class ActionResultExtensions
    {
        public static OperationResponse GetOperationResponse(this ActionResult<OperationResponse> result)
        {
            if (result.Value != null)
            {
                return result.Value;
            }
            else if (result.Result != null)
            {
                if ((result.Result as ObjectResult)?.Value is OperationResponse res)
                {
                    return res;
                }
                else
                {
                    return result.Value;
                }
            }
            else
            {
                return null;
            }
        }

        public static T GetResult<T>(this ActionResult<T> result)
            where T : class
        {
            if (result.Value != null)
            {
                return result.Value;
            }
            else if (result.Result != null)
            {
                if ((result.Result as ObjectResult)?.Value is T res)
                {
                    return res;
                }
                else
                {
                    return result.Value;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
