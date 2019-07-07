using Hood.Caching;
using Hood.Core;
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
    /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, it will then run a user check, and save the subscription info into the context pipeline in Items["AccountInfo"]
    /// </summary>
    /// <param name="Tiered">Determines whether a tiered subscription level is required, set as false if any (including addons or standalone subscriptions) are required.</param>
    /// <param name="Ids">The Ids of the subscription which you are checking for.</param>
    public class SubscriptionRequiredAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, it will then run a user check, and save the subscription info into the context pipeline in Items["AccountInfo"]
        /// </summary>
        /// <param name="Tiered">Determines whether a tiered subscription level is required, set as false if any (including addons or standalone subscriptions) are required.</param>
        /// <param name="Ids">The Ids of the subscription which you are checking for.</param>
        /// <param name="AddonsRequired">The Ids of the addon subscription which you are checking for. These will be checked for first and then the user is redirected to the purchase addon page if not present.</param>        /// 
        /// <param name="MinimumSubscription">The miminum tiered subscription level that is required. If set with SubscriptionIds, will require both (a matched Id and a minimum level of this parameter). This will also override the Tiered parameter, as a tiered subscription will be required to need a subscription above a minimum tier.</param>
        public SubscriptionRequiredAttribute(string SubscriptionIds = "", string Categories = "", string AddonsRequired = "", string Roles = "") : base(typeof(SubscriptionRequiredAttributeImpl))
        {
            if (SubscriptionIds == null)
                SubscriptionIds = "";
            var Ids = SubscriptionIds.Split(',').ToList();
            Ids.RemoveAll(str => string.IsNullOrEmpty(str));

            if (Categories == null)
                Categories = "";
            var CategoryList = Categories.Split(',').ToList();
            CategoryList.RemoveAll(str => string.IsNullOrEmpty(str));

            if (Roles == null)
                Roles = "";
            var RoleOverrides = Roles.Split(',').ToList();
            RoleOverrides.RemoveAll(str => string.IsNullOrEmpty(str));

            if (AddonsRequired == null)
                AddonsRequired = "";
            var Addons = AddonsRequired.Split(',').ToList();
            Addons.RemoveAll(str => string.IsNullOrEmpty(str));

            Arguments = new object[] { Ids.ToArray(), CategoryList.ToArray(), Addons.ToArray(), RoleOverrides.ToArray(), };
        }

        private class SubscriptionRequiredAttributeImpl : IActionFilter
        {
            private readonly List<string> _ids;
            private readonly List<string> _addons;
            private readonly List<string> _categories;
            private readonly List<string> _roles;
            private readonly UserManager<ApplicationUser> _userManager;
            public SubscriptionRequiredAttributeImpl(
                UserManager<ApplicationUser> userManager,
                string[] Ids,
                string[] CategoryList,
                string[] Addons,
                string[] RoleOverrides)
            {
                _ids = Ids.ToList();
                _categories = CategoryList.ToList();
                _userManager = userManager;
                _addons = Addons.ToList();
                _roles = RoleOverrides.ToList();
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (!Engine.Settings.Billing.CheckSubscriptionsOrThrow())
                {
                    throw new Exception("Subscriptions are not enabled");
                }

                // Set the redirect result for no subscriptions and subscription upgrade required
                BillingSettings billingSettings = Engine.Settings.Billing;
                IActionResult newsubResult = billingSettings.GetNewSubscriptionUrl(context.HttpContext);
                IActionResult changeResult = billingSettings.GetChangeSubscriptionUrl(context.HttpContext);

                // Set the redirect location
                IActionResult categoryResult = new RedirectToActionResult("New", "Subscriptions", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent()});
                if (_categories.Count == 1)
                    categoryResult = new RedirectToActionResult("New", "Subscriptions", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent(), category = _categories[0] });
                if (billingSettings.SubscriptionCreatePage.IsSet())
                {
                    UriBuilder baseUri = new UriBuilder(context.HttpContext.GetSiteUrl() + billingSettings.SubscriptionCreatePage.TrimStart('/'));
                    string queryToAppend = string.Format("returnUrl={0}", context.HttpContext.Request.Path.ToUriComponent());
                    if (baseUri.Query != null && baseUri.Query.Length > 1)
                        baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
                    else
                        baseUri.Query = queryToAppend;
                    if (_categories.Count == 1)
                        baseUri.Query += "&category=" + _categories[0];
                    newsubResult = new RedirectResult(baseUri.ToString());
                }

                // If an addon is required, this takes preference, and should be purchased to continue, as it may be a standalone addon required.
                if (_addons.Count > 0)
                {
                    bool go = _ids.Count == 0;
                    foreach (string addon in _addons)
                    {
                        if (!context.HttpContext.User.IsSubscribed(addon))
                        {
                            IActionResult addonResult = new RedirectToActionResult("Addon", "Subscriptions", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent(), required = addon });
                            if (billingSettings.SubscriptionAddonPage.IsSet())
                            {
                                UriBuilder baseUri = new UriBuilder(context.HttpContext.GetSiteUrl() + billingSettings.SubscriptionAddonPage.TrimStart('/'));
                                string queryToAppend = string.Format("returnUrl={0}&required={1}", context.HttpContext.Request.Path.ToUriComponent(), addon);
                                if (baseUri.Query != null && baseUri.Query.Length > 1)
                                    baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
                                else
                                    baseUri.Query = queryToAppend;
                                addonResult = new RedirectResult(baseUri.ToString());
                            }
                            context.Result = addonResult;
                            return;
                        }
                    }
                    if (go)
                        return;
                }

                // If they are in an override role, let them through.
                if (_userManager.GetRolesAsync(context.HttpContext.User.AccountInfo().AsUser()).Result.Any(r => _roles.Contains(r)))
                    return;

                // If we reached this point then we are on the hunt for a tiered subscription of some kind.
                // Check for general tiered subscriptions - one is always required to proceed further.
                if (!context.HttpContext.User.HasActiveSubscription())
                {
                    context.Result = newsubResult;
                    return;
                }

                // If a category is set, then check that one of the user's subscriptions has that category.
                if (_categories.Count > 0 && !_categories.Any(i => context.HttpContext.User.IsSubscribedToCategory(i)))
                {
                    context.Result = categoryResult;
                    return;
                }

                // If an Id is set, then check that one of the user's subscriptions has that Id.
                if (_ids.Count > 0 && !_ids.Any(i => context.HttpContext.User.IsSubscribed(i)))
                {
                    context.Result = changeResult;
                    return;
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
