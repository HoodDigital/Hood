using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using Hood.Services;
using Stripe;
using Hood.Extensions;
using System.IO;
using Hood.Filters;
using Hood.Enums;
using Newtonsoft.Json;
using System.Text;

namespace Hood.Controllers
{
    [Authorize]
    [StripeRequired]
    public class SubscriptionsController : Controller
    {
        private readonly IAccountRepository _auth;
        private readonly IBillingService _billing;
        private readonly IStripeWebHookService _webHooks;

        public SubscriptionsController(
            IAccountRepository auth,
            IBillingService billing,
            IStripeWebHookService webHooks)
        {
            _auth = auth;
            _billing = billing;
            _webHooks = webHooks;
        }

        [HttpGet]
        [Route("account/subscriptions/")]
        [SubscriptionRequired(Tiered: true)]
        public IActionResult Index()
        {
            AccountInfo account = HttpContext.GetAccountInfo();
            SubscriptionModel model = new SubscriptionModel();
            return View(model);
        }

        [HttpGet]
        [Route("account/subscriptions/addon-required/")]
        [SubscriptionRequired(AddonsRequired: "intro")]
        public IActionResult AddonArea()
        {
            AccountInfo account = HttpContext.GetAccountInfo();
            SubscriptionModel model = new SubscriptionModel();
            return View(model);
        }

        [HttpGet]
        [Route("account/subscriptions/change/")]
        [SubscriptionRequired()]
        public async Task<IActionResult> Change(BillingMessage? message = null)
        {
            AccountInfo account = HttpContext.GetAccountInfo();
            SubscriptionModel model = new SubscriptionModel();
            model.User = account.User;
            model.Plans = await _auth.GetLevels();
            model.Addons = await _auth.GetAddons();
            model.Message = message;
            return View(model);
        }

        [HttpGet]
        [Route("account/subscriptions/new/")]
        public async Task<IActionResult> New(string returnUrl = null, BillingMessage? message = null)
        {
            SubscriptionModel model = await GetCreateModel(returnUrl);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/subscriptions/new/")]
        public async Task<IActionResult> New(SubscriptionModel model, string returnUrl = null)
        {
            try
            {
                await _auth.CreateUserSubscription(model.PlanId, model.StripeToken, model.CardId);
                if (returnUrl.IsSet())
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Billing", new { message = BillingMessage.SubscriptionCreated });
            }
            catch (Exception ex)
            {
                model = await GetCreateModel(returnUrl);
                model.Message = ex.Message.ToEnum(BillingMessage.Error);
                return View(model);
            }
        }

        [HttpGet]
        [Route("account/subscriptions/addon/")]
        public async Task<IActionResult> Addon(string required, string returnUrl = null, BillingMessage? message = null)
        {
            SubscriptionModel model = await GetCreateModel(returnUrl);
            return View(model);
        }

        private async Task<SubscriptionModel> GetCreateModel(string returnUrl)
        {
            AccountInfo account = HttpContext.GetAccountInfo();
            SubscriptionModel model = new SubscriptionModel();
            model.User = account.User;
            model.Plans = await _auth.GetLevels();
            model.Addons = await _auth.GetAddons();
            model.Customer = await _auth.LoadCustomerObject(account.User.StripeId, true);
            ViewData["ReturnUrl"] = returnUrl;
            return model;
        }

        [HttpGet]
        [Route("account/subscriptions/upgrade/")]
        [SubscriptionRequired]
        public async Task<IActionResult> Upgrade(int id, int plan)
        {
            try
            {
                await _auth.UpgradeUserSubscription(id, plan);
                return RedirectToAction("Index", "Billing", new { message = BillingMessage.SubscriptionUpdated });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Billing", new { message = ex.Message.ToEnum(BillingMessage.Error) });
            }
        }

        [Route("account/subscriptions/cancel/")]
        [SubscriptionRequired]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _auth.CancelUserSubscription(id);
                return RedirectToAction("Index", "Billing", new { message = BillingMessage.SubscriptionCancelled });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Billing", new { message = ex.Message.ToEnum(BillingMessage.Error) });
            }
        }

        [Route("account/subscriptions/remove/")]
        [SubscriptionRequired]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                await _auth.RemoveUserSubscription(id);
                return RedirectToAction("Index", "Billing", new { message = BillingMessage.SubscriptionEnded });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Billing", new { message = ex.Message.ToEnum(BillingMessage.Error) });
            }
        }

        [Route("account/subscriptions/reactivate/")]
        [SubscriptionRequired]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                await _auth.ReactivateUserSubscription(id);
                return RedirectToAction("Index", "Billing", new { message = BillingMessage.SubscriptionReactivated });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Billing", new { message = ex.Message.ToEnum(BillingMessage.Error) });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("stripe/webhooks")]
        public async Task<StatusCodeResult> WebHooks()
        {
            var json = new StreamReader(Request.Body).ReadToEnd();
            try
            {
                await _webHooks.ProcessEvent(json);
                return new StatusCodeResult(202);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }
        private string FormatLog(string v)
        {
            return DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + ": " + v + Environment.NewLine;
        }
    }

}


