using Hood.BaseTypes;
using Hood.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class BillingSettings : SaveableModel
    {
        // Paypal
        [Display(Name = "Enable PayPal", Description = "To enable paypal, you must also supply a Paypal Client Id and a Client Secret in your application settings (appsettings.json or Env variables).")]
        public bool EnablePayPal { get; set; }
        [Display(Name = "PayPal Sandbox Mode", Description = "Transactions processed will use the PayPal sandbox.")]
        public bool PayPalSandboxMode { get; set; }
        [Display(Name = "PayPal Client Id")]
        public string PayPalClientId { get; set; }
        [Display(Name = "PayPal Secret")]
        public string PayPalSecret { get; set; }

        // SagePay
        [Display(Name = "Enable SagePay", Description = "To enable stripe, you must also supply a Stripe Key and a Stripe Public Key in your application settings (appsettings.json or Env variables).")]
        public bool EnableSagePay { get; set; }
        [Display(Name = "SagePay Mode")]
        public string SagePayMode { get; set; }

        [Display(Name = "SagePay Endpoint",Description = "This would normally be <code>https://pi-live.sagepay.com/api/v1</code>")]
        public string SagePayEndpoint { get; set; }
        [Display(Name = "SagePay Key")]
        public string SagePayKey { get; set; }
        [Display(Name = "SagePay Password")]
        public string SagePayPassword { get; set; }
        [Display(Name = "SagePay Vendor Name")]
        public string SagePayVendorName { get; set; }

        // SagePay Testing
        [Display(Name = "SagePay Testing Endpoint", Description = "This would normally be <code>https://pi-test.sagepay.com/api/v1</code>")]
        public string SagePayTestingEndpoint { get; set; }
        [Display(Name = "SagePay Testing Key")]
        public string SagePayTestingKey { get; set; }
        [Display(Name = "SagePay Testing Password")]
        public string SagePayTestingPassword { get; set; }
        [Display(Name = "SagePay Testing Vendor Name")]
        public string SagePayTestingVendorName { get; set; }

        // Store
        [Display(Name = "Enable Shopping Cart / Checkout", Description = "To enable the shopping cart system, you must also enable Stripe or Paypal in your application settings (appsettings.json or Env variables).")]
        public bool EnableCart { get; set; }

        // Stripe
        [Display(Name = "Enable Stripe", Description = "To enable stripe, you must also supply a Stripe Key and a Stripe Public Key in your application settings (appsettings.json or Env variables).")]
        public bool EnableStripe { get; set; }
        [Display(Name = "Enable Subscriptions", Description = "To enable subscriptions, you must also enable Stripe and supply a Stripe Key and a Stripe Public Key in your application settings (appsettings.json or Env variables).")]
        public bool EnableSubscriptions { get; set; }
        [Display(Name = "Stripe Test Mode", Description = "Transactions processed will use the test service and process using your Test Api keys.")]
        public bool EnableStripeTestMode { get; set; }
        [Display(Name = "Subscription Upgrade Page", Description = "Use this area to change the location of the upgrade subscriptions page. Use a relative url such as [ /subscriptions/change ] - <strong>Default: /subscriptions/change</strong>")]
        public string SubscriptionUpgradePage { get; set; }
        [Display(Name = "Subscription Create Page", Description = "Use this area to change the location of the upgrade subscriptions page. Use a relative url such as [ /subscriptions/get ] - <strong>Default: /subscriptions/get</strong>")]
        public string SubscriptionCreatePage { get; set; }
        [Display(Name = "Subscription Addon Page", Description = "Use this area to change the location of the upgrade subscriptions page. Use a relative url such as [ /subscriptions/addon ] - <strong>Default: /subscriptions/addon</strong>")]
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
        [Display(Name = "Stripe Webhook Secret")]
        public string StripeWebhookSecret { get; set; }

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
        public bool IsCartEnabled
        {
            get
            {
                if (!EnableStripe && !EnablePayPal)
                    return false;
                if (!EnableCart)
                    return false;
                if (!StripeSetup && !PayPalSetup)
                    return false;
                return true;
            }
        }

        [JsonIgnore]
        private bool StripeSetup
        {
            get
            {
                if (!StripeLiveKey.IsSet() ||
                    !StripeLivePublicKey.IsSet() ||
                    !StripeTestKey.IsSet() ||
                    !StripeTestPublicKey.IsSet())
                    return false;
                return true;
            }
        }

        [JsonIgnore]
        private bool PayPalSetup
        {
            get
            {
                if (!PayPalSecret.IsSet() ||
                    !PayPalClientId.IsSet())
                    return false;
                return true;
            }
        }
        public bool IsBillingEnabled
        {
            get
            {
                if (!EnableStripe && !EnablePayPal)
                    return false;
                if (!StripeSetup && !PayPalSetup)
                    return false;
                return true;
            }
        }

        public bool IsStripeEnabled
        {
            get
            {
                if (!EnableStripe)
                    return false;
                if (!StripeSetup)
                    return false;
                return true;
            }
        }
        public bool IsPaypalEnabled
        {
            get
            {
                if (!EnablePayPal)
                    return false;
                if (!PayPalSetup)
                    return false;
                return true;
            }
        }
        /// <summary>
        /// This will check all required settings are correct for any billing services to work. Will throw an <see cref="Exception"/> when not setup explaining how to setup correctly.
        /// </summary>
        public bool CheckBillingOrThrow()
        {
            if (!EnableStripe && !EnablePayPal)
                throw new Exception("Stripe or PayPal are not enabled, please enable one of them in the administrators area, under Settings > Billing Settings.");
            if (EnableStripe && !StripeSetup)
                throw new Exception("Stripe is not set up correctly, please ensure you have set the correct  settings in the administrators area, under Settings > Billing Settings.");
            if (EnablePayPal && !PayPalSetup)
                throw new Exception("PayPal is not set up correctly, please ensure you have set the correct  settings in the administrators area, under Settings > Billing Settings.");
            return true;
        }
        /// <summary>
        /// This will check all required settings are correct for Stripe to work, also checks that it is enabled. Will throw an <see cref="Exception"/> when not setup explaining how to setup correctly.
        /// </summary>
        public bool CheckStripeOrThrow()
        {
            if (!EnableStripe)
                throw new Exception("Stripe is not enabled, please enable it in the administrators area, under Settings > Billing Settings.");
            if (!StripeSetup)
                throw new Exception("Stripe is not set up correctly, please ensure you have set the correct settings in the administrators area, under Settings > Billing Settings.");
            return true;
        }
        /// <summary>
        /// This will check all required settings are correct for PayPal to work, also checks that it is enabled. Will throw an <see cref="Exception"/> when not setup explaining how to setup correctly.
        /// </summary>
        public bool CheckPaypalOrThrow()
        {
            if (!EnablePayPal)
                return false;
            if (!PayPalSetup)
                return false;
            return true;
        }
        public bool IsSubscriptionsEnabled
        {
            get
            {
                if (!EnableStripe)
                    return false;
                if (!EnableSubscriptions)
                    return false;
                return true;
            }
        }
        /// <summary>
        /// This will check all required settings are correct for subscriptions to work. Will throw an <see cref="Exception"/> when not setup explaining how to setup correctly.
        /// </summary>
        public bool CheckSubscriptionsOrThrow()
        {
            CheckStripeOrThrow();
            if (EnableSubscriptions)
                return true;
            else
                throw new Exception("Subscriptions are not enabled, please enable them in the administrators area, under Settings > Billing Settings.");
        }
        /// <summary>
        /// This will check all required settings are correct for the cart to work. Will throw an <see cref="Exception"/> when not setup explaining how to setup correctly.
        /// </summary>
        public bool CheckCartOrThrow()
        {
            CheckBillingOrThrow();
            if (!EnableCart)
                throw new Exception("The shopping cart & checkout is not enabled, please enable it in the administrators area, under Settings > Billing Settings.");
            else
                if (StripeSetup || PayPalSetup)
                    return true;
                else
                    throw new Exception("Stripe or PayPal are not set up correctly, please ensure you have set the correct  settings in the administrators area, under Settings > Billing Settings.");
        }
    }
}
