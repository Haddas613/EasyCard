using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace CheckoutPortal.Models
{
    [ModelBinder(BinderType = typeof(CardExpirationBinder))]
    public class CardExpirationMVC
    {
        public CardExpirationMVC()
        {
        }

        [Range(20, 90)]
        [Required(AllowEmptyStrings = false)]
        public int? Year { get; set; }

        [Range(1, 12)]
        [Required(AllowEmptyStrings = false)]
        public int? Month { get; set; }

        public override string ToString()
        {
            return CreditCardHelpers.FormatCardExpiration(ToDate());
        }

        public DateTime? ToDate()
        {
            if (Year.HasValue && Month.HasValue)
            {
                return new DateTime(Year.Value + 2000, Month.Value, 1);
            }
            else
            {
                return null;
            }
        }

        public bool Expired
        {
            get
            {
                return ToDate()?.Date.AddMonths(1) < DateTime.UtcNow;
            }

            set
            {
            }
        }
    }

    public class CardExpirationBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            // TODO: failed case
            var model = CreditCardHelpers.ParseCardExpiration(value);

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
