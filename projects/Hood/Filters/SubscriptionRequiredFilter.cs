using Hood.Extensions;
using Hood.Models;
using Hood.Services;
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
        public SubscriptionRequiredAttribute(bool Tiered = false, string SubscriptionIds = "", string MinimumSubscription = "", string AddonsRequired = "") : base(typeof(SubscriptionRequiredAttributeImpl))
        {
            if (SubscriptionIds == null)
                SubscriptionIds = "";
            var Ids = SubscriptionIds.Split(',').ToList();
            Ids.RemoveAll(str => string.IsNullOrEmpty(str));

            if (AddonsRequired == null)
                AddonsRequired = "";
            var Addons = AddonsRequired.Split(',').ToList();
            Addons.RemoveAll(str => string.IsNullOrEmpty(str));

            Arguments = new object[] { Ids.ToArray(), Addons.ToArray(), Tiered, MinimumSubscription };
        }

        private class SubscriptionRequiredAttributeImpl : IActionFilter
        {
            private readonly ILogger _logger;
            private readonly IBillingService _billing;
            private readonly ISettingsRepository _settings;
            private readonly List<string> _ids;
            private readonly List<string> _addons;
            private readonly bool _tiered;
            private readonly string _minimum;
            private readonly IAccountRepository _auth;

            public SubscriptionRequiredAttributeImpl(
                IAccountRepository auth, 
                ILoggerFactory loggerFactory, 
                IBillingService billing, 
                ISettingsRepository site, 
                string[] Ids, 
                string[] Addons, 
                bool Tiered, 
                string MinimumSubscription)
            {
                _auth = auth;
                _logger = loggerFactory.CreateLogger<SubscriptionRequiredAttribute>();
                _billing = billing;
                _settings = site;
                _ids = Ids.ToList();
                _addons = Addons.ToList();
                _tiered = Tiered;
                _minimum = MinimumSubscription;
                if (_minimum.IsSet())
                    _tiered = true;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var subscriptionsEnabled = _settings.SubscriptionsEnabled();
                if (!subscriptionsEnabled.Succeeded)
                {
                    throw new Exception(subscriptionsEnabled.ErrorString);
                }

                // Load the account information from the global context (set in the global AccountFilter)
                AccountInfo info = context.HttpContext.GetAccountInfo();

                // Set the redirect location
                BillingSettings billingSettings = _settings.GetBillingSettings();
                IActionResult result = new RedirectToActionResult("New", "Subscriptions", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent() });
                if (billingSettings.SubscriptionCreatePage.IsSet())
                {
                    UriBuilder baseUri = new UriBuilder(context.HttpContext.GetSiteUrl() + billingSettings.SubscriptionCreatePage.TrimStart('/'));
                    string queryToAppend = string.Format("returnUrl={0}", context.HttpContext.Request.Path.ToUriComponent());
                    if (baseUri.Query != null && baseUri.Query.Length > 1)
                        baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
                    else
                        baseUri.Query = queryToAppend;
                    result = new RedirectResult(baseUri.ToString());
                }

                IActionResult changeResult = new RedirectToActionResult("Change", "Subscriptions", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent() });
                if (billingSettings.SubscriptionUpgradePage.IsSet())
                {
                    UriBuilder baseUri = new UriBuilder(context.HttpContext.GetSiteUrl() + billingSettings.SubscriptionUpgradePage.TrimStart('/'));
                    string queryToAppend = string.Format("returnUrl={0}", context.HttpContext.Request.Path.ToUriComponent());
                    if (baseUri.Query != null && baseUri.Query.Length > 1)
                        baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
                    else
                        baseUri.Query = queryToAppend;
                    changeResult = new RedirectResult(baseUri.ToString());
                }

                // If an addon is required, this takes preference, and should be purchased to continue, as it may be a standalone addon required.
                if (_addons.Count > 0)
                {
                    bool go = _ids.Count == 0 && !_tiered && !_minimum.IsSet();
                    foreach (string addon in _addons)
                    {
                        if (!info.IsSubscribed(addon))
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

                // If we reached this point then we are on the hunt for a tiered subscription of some kind.

                // Check for general tiered subscriptions - one is always required to proceed further.
                if (!info.HasTieredSubscription)
                {
                    context.Result = result;
                    return;
                }

                // If an Id is set, then check that one of the user's subscriptions has that Id.
                if (_ids.Count > 0 && !_ids.Any(i => info.IsSubscribed(i)))
                {
                    context.Result = changeResult;
                    return;
                }

                // If tiered is set, check if any of the user's subscriptions are tiered.
                if (_tiered && !info.ActiveSubscriptions.Any(ss => ss.Tiered))
                {
                    context.Result = changeResult;
                    return;
                }

                // If an Minimum is set, check the user has a subcription of a higher level
                if (_tiered && _minimum.IsSet())
                {
                    Subscription sub = _auth.GetSubscriptionByStripeId(_minimum).Result;
                    if (sub == null)
                        throw new Exception("A subscription with Id \"" + _minimum + "\" could not be found.");
                    if (!info.ActiveSubscriptions.Any(ss => ss.Level >= sub.Level))
                    {
                        context.Result = changeResult;
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
