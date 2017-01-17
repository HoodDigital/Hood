using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

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
            private readonly ILogger _logger;
            private readonly IBillingService _billing;
            private readonly ISiteConfiguration _site;

            public StripeRequiredAttributeImpl(ILoggerFactory loggerFactory, IBillingService billing, ISiteConfiguration site)
            {
                _logger = loggerFactory.CreateLogger<StripeRequiredAttribute>();
                _billing = billing;
                _site = site;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var result = _site.StripeEnabled();
                if (!result.Succeeded)
                {
                    throw new Exception(result.ErrorString);
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
