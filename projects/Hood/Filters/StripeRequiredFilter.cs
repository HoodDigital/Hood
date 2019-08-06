using Hood.Core;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Hood.Filters
{
    /// <summary>
    /// This checks for the stripe feature, if it is not installed correctly or enabled this will short circuit the controller/action.
    /// </summary>
    public class StripeRequiredAttribute : TypeFilterAttribute
    {
        public StripeRequiredAttribute() : base(typeof(StripeRequiredAttributeImpl))
        {
        }

        private class StripeRequiredAttributeImpl : IActionFilter
        {

            public StripeRequiredAttributeImpl()
            {
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (!Engine.Settings.Billing.CheckStripeOrThrow())
                {
                    // Stripe is not enabled, exception will have thrown.
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
