using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Validation
{
    public static class ValidationExtensions
    {
        public static OperationResponse BuildOperationResponseFromModelState(this ModelStateDictionary modelState, string correlationId)
        {
            var result = new OperationResponse { Status = StatusEnum.Error, Message = ApiMessages.ValidationErrors, CorrelationId = correlationId };

            foreach (var state in modelState)
            {
                var error = new Error
                {
                    Code = state.Key
                };

                List<string> errors = new List<string>();

                foreach (var stateError in state.Value.Errors)
                {
                    if (stateError.Exception != null)
                    {
                        errors.Add(stateError.Exception.Message);
                    }
                    else
                    {
                        errors.Add(stateError.ErrorMessage);
                    }
                }

                error.Description = string.Join("; ", errors);

                result.Errors.Add(error);
            }

            return result;
        }
    }
}
