using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.Presentation.ActionFilters
{
    // A custom action filter for request validation
    public class ValidationFilterAttribute : IActionFilter
    {
        public ValidationFilterAttribute()
        {
        }

        // Executes before the action method is invoked
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Get the name of the action and controller
            var action = context.RouteData.Values["action"];
            var controller = context.RouteData.Values["controller"];

            // Find the parameter in the action arguments that contains "Dto" in its name
            var param = context.ActionArguments
                .SingleOrDefault(x => x.Value.ToString().Contains("Dto")).Value;

            // If the parameter is null, return a bad request with an error message
            if (param is null)
            {
                context.Result = new BadRequestObjectResult($"Object is null. Controller: {controller}, action: {action}");
                return;
            }

            // If the model state is not valid, return an unprocessable entity with the model state errors
            if (!context.ModelState.IsValid)
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
        }

        // Executes after the action method is invoked
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // This method is not implemented
            throw new NotImplementedException();
        }
    }
}
