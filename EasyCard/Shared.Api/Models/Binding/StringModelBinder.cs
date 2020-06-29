using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Api.Models.Binding
{
    public class StringModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelName = bindingContext.ModelName;

            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(
                valueProviderResult.FirstValue.TrimAndNullIfWhiteSpace());

            return Task.CompletedTask;
        }
    }
}
