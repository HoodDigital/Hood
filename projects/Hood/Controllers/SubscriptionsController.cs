using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using Hood.Services;
using Hood.Extensions;
using System.IO;
using Hood.Filters;
using Hood.Enums;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Hood.Controllers
{
    [Authorize]
    [StripeRequired]
    //[Area("Hood")]
    public class SubscriptionsController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {

        private readonly IStripeWebHookService _webHooks;
        public SubscriptionsController(
            IStripeWebHookService webHooks
            )
        {
            _webHooks = webHooks;
        }

        [HttpGet]
        [SubscriptionRequired(Roles: "SuperUser")]
        public async Task<IActionResult> Index(BillingMessage? message = null)
        {
            AccountInfo account = HttpContext.GetAccountInfo();
            SubscriptionModel model = new SubscriptionModel()
            {
                User = account.User,
                Plans = await _auth.GetSubscriptionPlanLevels(),
                Addons = await _auth.GetSubscriptionPlanAddons()
            };
            model.AddBillingMessage(message);
            return View(model);
        }

        [HttpGet]
        [SubscriptionRequired(Roles: "SuperUser")]
        public async Task<IActionResult> Change(string category, BillingMessage? message = null)
        {
            AccountInfo account = HttpContext.GetAccountInfo();
            SubscriptionModel model = new SubscriptionModel()
            {
                User = account.User,
                Category = category,
                Plans = await _auth.GetSubscriptionPlanLevels(category),
                Addons = await _auth.GetSubscriptionPlanAddons()
            };
            model.AddBillingMessage(message);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> New(string category, string returnUrl = null, BillingMessage? message = null)
        {
            SubscriptionModel model = await GetCreateModel(category, returnUrl);
            model.AddBillingMessage(message);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(SubscriptionModel model, string returnUrl = null)
        {
            try
            {
                var sub = await _auth.CreateUserSubscription(model.PlanId, model.StripeToken, model.CardId);
                _eventService.triggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs($"New subscription created by user {User.Identity.Name}", sub));
                returnUrl = GetReturnUrl(BillingMessage.SubscriptionCreated, sub.SubscriptionId, returnUrl);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                model = await GetCreateModel(model.Category, returnUrl);
                BillingMessage bm = BillingMessage.Null;
                if (Enum.TryParse(ex.Message, out bm))
                {
                    model.AddBillingMessage(bm);
                }
                else
                {
                    model.AddBillingMessage(BillingMessage.StripeError);
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Addon(string category, string required, string returnUrl = null, BillingMessage? message = null)
        {
            SubscriptionModel model = await GetCreateModel(category, returnUrl);
            model.AddBillingMessage(message);
            return View(model);
        }


        [HttpGet]
        [SubscriptionRequired]
        public async Task<IActionResult> Upgrade(int id, int plan)
        {
            try
            {
                var sub = await _auth.UpgradeUserSubscription(id, plan);
                _eventService.triggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs($"Subscription upgraded by user {User.Identity.Name}", sub));
                var returnUrl = GetReturnUrl(BillingMessage.SubscriptionUpdated, sub.SubscriptionId, null);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                var returnUrl = GetReturnUrl(BillingMessage.ErrorUpdatingSubscription, id, null, ex.Message);
                return Redirect(returnUrl);
            }
        }

        [SubscriptionRequired]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var sub = await _auth.CancelUserSubscription(id);
                _eventService.triggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs($"Subscription cancelled by user {User.Identity.Name}", sub));
                var returnUrl = GetReturnUrl(BillingMessage.SubscriptionCancelled, sub.SubscriptionId, null);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                var returnUrl = GetReturnUrl(BillingMessage.ErrorCancellingSubscription, id, null, ex.Message);
                return Redirect(returnUrl);
            }
        }

        [SubscriptionRequired]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                var sub = await _auth.RemoveUserSubscription(id);
                _eventService.triggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs($"Subscription removed by user {User.Identity.Name}", sub));
                var returnUrl = GetReturnUrl(BillingMessage.SubscriptionEnded, sub.SubscriptionId, null);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                var returnUrl = GetReturnUrl(BillingMessage.ErrorRemovingSubscription, id, null, ex.Message);
                return Redirect(returnUrl);
            }
        }

        [SubscriptionRequired]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                var sub = await _auth.ReactivateUserSubscription(id);
                _eventService.triggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs($"Subscription re-activated by user {User.Identity.Name}", sub));
                var returnUrl = GetReturnUrl(BillingMessage.SubscriptionReactivated, sub.SubscriptionId, null);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                var returnUrl = GetReturnUrl(BillingMessage.ErrorReactivatingSubscription, id, null, ex.Message);
                return Redirect(returnUrl);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("stripe/webhooks/")]
        public StatusCodeResult WebHooks()
        {
            var json = new StreamReader(Request.Body).ReadToEnd();
            try
            {
                _webHooks.ProcessEvent(json);
                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                return new StatusCodeResult(202);
            }
        }

        private async Task<SubscriptionModel> GetCreateModel(string category, string returnUrl)
        {
            AccountInfo account = HttpContext.GetAccountInfo();
            SubscriptionModel model = new SubscriptionModel()
            {
                User = account.User,
                Category = category,
                Plans = await _auth.GetSubscriptionPlanLevels(category),
                Addons = await _auth.GetSubscriptionPlanAddons(),
                Customer = await _auth.LoadCustomerObject(account.User.StripeId, true)
            };
            ViewData["ReturnUrl"] = returnUrl;
            return model;
        }

        private string GetReturnUrl(BillingMessage? message, int? planId, string returnUrl, string errors = null)
        {
            if (!returnUrl.IsSet())
            {
                returnUrl = Url.Action("Change", "Subscriptions");
            }
            var newParams = new Dictionary<string, string>();
            if (message.HasValue)
                newParams.Add("message", message.ToString());
            if (planId.HasValue)
                newParams.Add("plan", planId.Value.ToString());
            if (errors.IsSet())
                newParams.Add("errors", errors);
            if (!returnUrl.IsAbsoluteUrl())
                returnUrl = ControllerContext.HttpContext.GetSiteUrl() + returnUrl.TrimStart('/');             
            var uri = returnUrl.AddParameterToUrl(newParams);
            return uri.ToString();
        }

    }

}


