using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Filters;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    [Authorize]
    [StripeRequired]
    //[Area("Hood")]
    public class SubscriptionsController : BaseController
    {

        private readonly IStripeWebHookService _webHooks;
        public SubscriptionsController(IStripeWebHookService webHooks)
            : base()
        {
            _webHooks = webHooks;
        }

        [HttpGet]
        [SubscriptionRequired(Roles: "SuperUser")]
        public async Task<IActionResult> Index()
        {
            SubscriptionModel model = new SubscriptionModel()
            {
                Plans = (await _account.GetSubscriptionPlansAsync(new SubscriptionPlanListModel() { PageSize = int.MaxValue, Addon = false })).List,
                Addons = (await _account.GetSubscriptionPlansAsync(new SubscriptionPlanListModel() { PageSize = int.MaxValue, Addon = true })).List
            };
            return View(model);
        }

        [HttpGet]
        [SubscriptionRequired(Roles: "SuperUser")]
        public async Task<IActionResult> Change(int? groupId)
        {
            SubscriptionModel model = new SubscriptionModel()
            {
                GroupId = groupId,
                Plans = (await _account.GetSubscriptionPlansAsync(new SubscriptionPlanListModel() { PageSize = int.MaxValue, ProductId = groupId, Addon = false })).List,
                Addons = (await _account.GetSubscriptionPlansAsync(new SubscriptionPlanListModel() { PageSize = int.MaxValue, Addon = true })).List
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> New(int? groupId, string returnUrl = null)
        {
            SubscriptionModel model = await GetCreateModel(groupId, returnUrl);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(SubscriptionModel model, string returnUrl = null)
        {
            try
            {
                var sub = await _account.CreateUserSubscriptionAsync(model.PlanId, model.StripeToken, model.CardId);

                SaveMessage = $"New subscription created by user {User.Identity.Name}";
                MessageType = AlertType.Success;
                _eventService.TriggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs(SaveMessage, sub));

                returnUrl = GetReturnUrl(sub.SubscriptionId, returnUrl);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                model = await GetCreateModel(model.GroupId, returnUrl);
                SaveMessage = $"Error";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Addon(int? groupId, string returnUrl = null)
        {
            SubscriptionModel model = await GetCreateModel(groupId, returnUrl);
            return View(model);
        }


        [HttpGet]
        [SubscriptionRequired]
        public async Task<IActionResult> Upgrade(int id, int plan)
        {
            try
            {
                var sub = await _account.SwitchUserSubscriptionAsync(id, plan);

                SaveMessage = $"Subscription upgraded by user {User.Identity.Name}";
                MessageType = AlertType.Success;
                _eventService.TriggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs(SaveMessage, sub));

                var returnUrl = GetReturnUrl(sub.SubscriptionId);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error upgrading a subscription.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);

                var returnUrl = GetReturnUrl(id);
                return Redirect(returnUrl);
            }
        }

        [SubscriptionRequired]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var sub = await _account.CancelUserSubscriptionAsync(id);

                SaveMessage = $"Subscription cancelled by user {User.Identity.Name}";
                MessageType = AlertType.Success;
                _eventService.TriggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs(SaveMessage, sub));

                var returnUrl = GetReturnUrl(sub.SubscriptionId);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error cancelling a subscription.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);

                var returnUrl = GetReturnUrl(id);
                return Redirect(returnUrl);
            }
        }

        [SubscriptionRequired]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                var sub = await _account.CancelUserSubscriptionAsync(id, false);

                SaveMessage = $"Subscription removed by user {User.Identity.Name}";
                MessageType = AlertType.Success;
                _eventService.TriggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs(SaveMessage, sub));

                var returnUrl = GetReturnUrl(sub.SubscriptionId);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error cancelling a subscription.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);

                var returnUrl = GetReturnUrl(id);
                return Redirect(returnUrl);
            }
        }

        [SubscriptionRequired]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                var sub = await _account.ReactivateUserSubscriptionAsync(id);

                SaveMessage = $"Subscription re-activated by user {User.Identity.Name}";
                MessageType = AlertType.Success;
                _eventService.TriggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs(SaveMessage, sub));

                var returnUrl = GetReturnUrl(sub.SubscriptionId);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error reactivating a subscription.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);

                var returnUrl = GetReturnUrl(id);
                return Redirect(returnUrl);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("stripe/webhooks/")]
        public async Task<StatusCodeResult> WebHooks()
        {
            var json = new StreamReader(Request.Body).ReadToEnd();
            
            try
            {
                if (!Engine.Settings.Billing.StripeWebhookSecret.IsSet())
                    throw new Exception("Cannot process a signed webhook without a Stripe Webhook Secret. Set one in the Billing Settings to fix this.");

                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], Engine.Settings.Billing.StripeWebhookSecret);
                await _webHooks.ProcessEventAsync(stripeEvent);
                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
               return BadRequest();
            }
        }

        private async Task<SubscriptionModel> GetCreateModel(int? groupId, string returnUrl)
        {
            SubscriptionModel model = new SubscriptionModel()
            {
                GroupId = groupId,
                Plans = (await _account.GetSubscriptionPlansAsync(new SubscriptionPlanListModel() { PageSize = int.MaxValue, ProductId = groupId, Addon = false })).List,
                Addons = (await _account.GetSubscriptionPlansAsync(new SubscriptionPlanListModel() { PageSize = int.MaxValue, Addon = true })).List,
                Customer = await _account.GetCustomerObjectAsync(Engine.Account.StripeId)
            };
            ViewData["ReturnUrl"] = returnUrl;
            return model;
        }

        private string GetReturnUrl(int? planId, string returnUrl = null)
        {
            if (!returnUrl.IsSet())
            {
                returnUrl = Url.Action("Index", "Subscriptions");
            }
            var newParams = new Dictionary<string, string>();
            if (planId.HasValue)
                newParams.Add("plan", planId.Value.ToString());
            if (!returnUrl.IsAbsoluteUrl())
                returnUrl = ControllerContext.HttpContext.GetSiteUrl() + returnUrl.TrimStart('/');             
            var uri = returnUrl.AddParameterToUrl(newParams);
            return uri.ToString();
        }

    }

}


