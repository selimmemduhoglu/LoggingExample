using Microsoft.AspNetCore.Mvc.Filters;
using LoggingExample.Web.Extensions;

namespace LoggingExample.Web.Filters
{
    /// <summary>
    /// Controller action'lar için otomatik model validation
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // ModelState geçerli değilse validation exception fırlat
            context.ModelState.ThrowIfInvalid();
            
            base.OnActionExecuting(context);
        }
    }
} 