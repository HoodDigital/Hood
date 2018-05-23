using Hood.Caching;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Filters
{
    /// <summary>
    /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, then it checks what is required for forum access and fires the user to the right pages to upgrade if required.
    /// </summary>
    /// <param name="AccessRequired">Determines what type of access is required.</param>
    public class ForumAuthorizeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, then it checks what is required for forum access and fires the user to the right pages to upgrade if required.
        /// </summary>
        /// <param name="AccessRequired">Determines what type of access is required.</param>
        public ForumAuthorizeAttribute(ForumAccess AccessRequired = ForumAccess.View) : base(typeof(ForumAuthorizeFilterImpl))
        {
            Arguments = new object[] { AccessRequired };
        }

        private class ForumAuthorizeFilterImpl : IActionFilter
        {
            private readonly ILogger _logger;
            private readonly IBillingService _billing;
            private readonly ISettingsRepository _settings;

            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IAccountRepository _auth;
            private readonly RoleManager<IdentityRole> _roleManager;

            private readonly ForumAccess _access;

            public ForumAuthorizeFilterImpl(
                HoodDbContext db,
                IAccountRepository auth,
                ILoggerFactory loggerFactory,
                IBillingService billing,
                IHttpContextAccessor contextAccessor,
                IHoodCache cache,
                ISettingsRepository settings,
                RoleManager<IdentityRole> roleManager,
                UserManager<ApplicationUser> userManager,
               ForumAccess access)
            {
                _auth = new AccountRepository(db, settings, billing, contextAccessor, cache, userManager, roleManager);
                _logger = loggerFactory.CreateLogger<SubscriptionRequiredAttribute>();
                _billing = billing;
                _settings = settings;
                _userManager = userManager;
                _roleManager = roleManager;
                _access = access;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                ForumSettings _forum = _settings.GetForumSettings();

                // Login required stuff
                IActionResult loginResult = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent() });
                if (!context.HttpContext.User.Identity.IsAuthenticated && _access == ForumAccess.View && _forum.ViewingRequiresLogin)
                {
                    context.Result = loginResult;
                    return;
                }
                if (!context.HttpContext.User.Identity.IsAuthenticated && _access == ForumAccess.Post && _forum.PostingRequiresLogin)
                {
                    context.Result = loginResult;
                    return;
                }

                AccountInfo _account = context.HttpContext.GetAccountInfo();

                // Subscription required stuff
                var subscriptionsEnabled = _settings.SubscriptionsEnabled();
                if (!subscriptionsEnabled.Succeeded)
                {
                    throw new Exception(subscriptionsEnabled.ErrorString);
                }

                BillingSettings billingSettings = _settings.GetBillingSettings();
                IActionResult subscriptionResult = billingSettings.GetNewSubscriptionUrl(context.HttpContext);

                if (!_account.HasTieredSubscription && _access == ForumAccess.View && _forum.ViewingRequiresSubscription)
                {
                    context.Result = subscriptionResult;
                    return;
                }
                if (!_account.HasTieredSubscription && _access == ForumAccess.Post && _forum.PostingRequiresSubscription)
                {
                    context.Result = subscriptionResult;
                    return;
                }

                // Specific subscription required stuff
                IActionResult changeSubscriptionResult = billingSettings.GetChangeSubscriptionUrl(context.HttpContext);

                // TO DO
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
