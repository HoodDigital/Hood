using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hood.Filters
{
    /// <summary>
    /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, it will then run a user check, and save the subscription info into the context pipeline in Items["AccountInfo"]
    /// </summary>
    public class StripeAccountRequiredAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, it will then run a user check, and save the subscription info into the context pipeline in Items["AccountInfo"]
        /// </summary>
        /// <param name="id">The Ids of the subscription which you are checking for.</param>
        public StripeAccountRequiredAttribute() : base(typeof(StripeAccountRequiredAttributeImpl))
        {
        }

        private class StripeAccountRequiredAttributeImpl : IActionFilter
        {
            private readonly IStripeService _stripe;
            private readonly UserManager<ApplicationUser> _userManager;

            public StripeAccountRequiredAttributeImpl(
                IStripeService stripe,
                UserManager<ApplicationUser> userManager)
            {
                _stripe = stripe;
                _userManager = userManager;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (Core.Engine.Settings.Billing.CheckStripeOrThrow())
                {
                    var user = _userManager.GetUserAsync(context.HttpContext.User).Result;

                    if (!user.StripeId.IsSet())
                    {
                        context.Result = new RedirectToActionResult(nameof(BillingController.Index), "Billing", null);
                        return;
                    }

                    var customer = _stripe.GetCustomerByIdAsync(user.StripeId).Result;

                    if (customer == null)
                    {
                        context.Result = new RedirectToActionResult(nameof(BillingController.Index), "Billing", null);
                        return;
                    }
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
