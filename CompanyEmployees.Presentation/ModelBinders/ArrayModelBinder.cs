using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.ModelBinders
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Check if the model type is enumerable
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // Get the provided value from the value provider
            var providedValue = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName)
                .ToString();

            // Check if the provided value is empty
            if (string.IsNullOrEmpty(providedValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // Get the generic type of the model
            var genericType = bindingContext.ModelType
                .GetTypeInfo().GenericTypeArguments[0];

            // Get the converter for the generic type
            var converter = TypeDescriptor.GetConverter(genericType);

            // Split the provided value by comma and convert each value to the generic type
            var objectArray = providedValue.Split(new[] { "," },
                StringSplitOptions.RemoveEmptyEntries)
                .Select(x => converter.ConvertFromString(x.Trim()))
                .ToArray();

            // Create an array of the generic type and copy the converted values
            var genericArray = Array.CreateInstance(genericType, objectArray.Length);
            objectArray.CopyTo(genericArray, 0);

            // Set the model to the generic array
            bindingContext.Model = genericArray;

            // Set the model binding result as success
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
