using Hood.BaseTypes;
using Hood.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class BillingSettings : SaveableModel
    {
        // Paypal
        [Display(Name = "Enable PayPal")]
        public bool EnablePayPal { get; set; }
        [Display(Name = "PayPal Sandbox Mode")]
        public bool PayPalSandboxMode { get; set; }
        [Display(Name = "PayPal Client Id")]
        public string PayPalClientId { get; set; }
        [Display(Name = "PayPal Secret")]
        public string PayPalSecret { get; set; }

        // SagePay
        [Display(Name = "Enable SagePay")]
        public bool EnableSagePay { get; set; }
        [Display(Name = "SagePayMode")]
        public string SagePayMode { get; set; }

        [Display(Name = "SagePay Endpoint")]
        public string SagePayEndpoint { get; set; }
        [Display(Name = "SagePay Key")]
        public string SagePayKey { get; set; }
        [Display(Name = "SagePay Password")]
        public string SagePayPassword { get; set; }
        [Display(Name = "SagePay Vendor Name")]
        public string SagePayVendorName { get; set; }

        // SagePay Testing
        [Display(Name = "SagePay Testing Endpoint")]
        public string SagePayTestingEndpoint { get; set; }
        [Display(Name = "SagePay Testing Key")]
        public string SagePayTestingKey { get; set; }
        [Display(Name = "SagePay Testing Password")]
        public string SagePayTestingPassword { get; set; }
        [Display(Name = "SagePay Testing Vendor Name")]
        public string SagePayTestingVendorName { get; set; }

        // Store
        [Display(Name = "Enable Shopping Cart / Checkout")]
        public bool EnableCart { get; set; }

        // Stripe
        [Display(Name = "Enable Stripe")]
        public bool EnableStripe { get; set; }
        [Display(Name = "Enable Subscriptions")]
        public bool EnableSubscriptions { get; set; }
        [Display(Name = "Stripe Test Mode")]
        public bool EnableStripeTestMode { get; set; }
        [Display(Name = "Subscription Upgrad ePage")]
        public string SubscriptionUpgradePage { get; set; }
        [Display(Name = "Subscription Create Page")]
        public string SubscriptionCreatePage { get; set; }
        [Display(Name = "Subscription Addon Page")]
        public string SubscriptionAddonPage { get; set; }
        [Display(Name = "Webhook Logs")]
        public string SubscriptionWebhookLogs { get; set; }
        [Display(Name = "Stripe Live Key")]
        public string StripeLiveKey { get; set; }
        [Display(Name = "Stripe Live Public Key")]
        public string StripeLivePublicKey { get; set; }
        [Display(Name = "Stripe Test Key")]
        public string StripeTestKey { get; set; }
        [Display(Name = "Stripe Test Public Key")]
        public string StripeTestPublicKey { get; set; }

        internal IActionResult GetNewSubscriptionUrl(HttpContext context)
        {
            var result = new RedirectToActionResult("New", "Subscriptions", new { returnUrl = context.Request.Path.ToUriComponent() });
            if (SubscriptionCreatePage.IsSet())
            {
                UriBuilder baseUri = new UriBuilder(context.GetSiteUrl() + SubscriptionCreatePage.TrimStart('/'));
                string queryToAppend = string.Format("returnUrl={0}", context.Request.Path.ToUriComponent());
                if (baseUri.Query != null && baseUri.Query.Length > 1)
                    baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
                else
                    baseUri.Query = queryToAppend;
                return new RedirectResult(baseUri.ToString());
            }
            return result;
        }
        internal IActionResult GetChangeSubscriptionUrl(HttpContext context)
        {
            var changeResult = new RedirectToActionResult("Change", "Subscriptions", new { returnUrl = context.Request.Path.ToUriComponent() });
            if (SubscriptionUpgradePage.IsSet())
            {
                UriBuilder baseUri = new UriBuilder(context.GetSiteUrl() + SubscriptionUpgradePage.TrimStart('/'));
                string queryToAppend = string.Format("returnUrl={0}", context.Request.Path.ToUriComponent());
                if (baseUri.Query != null && baseUri.Query.Length > 1)
                    baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
                else
                    baseUri.Query = queryToAppend;
                return new RedirectResult(baseUri.ToString());
            }
            return changeResult;
        }
    }
}
