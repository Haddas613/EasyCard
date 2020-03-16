using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Validation
{
    public class ValidateModelStateFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var result = context.ModelState.BuildOperationResponseFromModelState(context.HttpContext.TraceIdentifier);

                context.Result = new ObjectResult(result) { StatusCode = (int)System.Net.HttpStatusCode.BadRequest };
            }
        }
    }
}
