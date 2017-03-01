using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hood.Models;
using Hood.Services;
using Stripe;
using Hood.Extensions;
using System.IO;
using Hood.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Hood.Enums;
using Newtonsoft.Json;
using System.Text;
using Hood.Caching;

namespace Hood.Controllers
{
    [Authorize]
    [StripeRequired]
    public class SubscriptionsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _email;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IContentRepository _data;
        private readonly IAuthenticationRepository _auth;
        private readonly ISubscriptionRepository _subs;
        private readonly ISiteConfiguration _site;
        private readonly IBillingService _billing;
        private readonly BasicSettings _info;
        private readonly IHoodCache _cache;

        public SubscriptionsController(
            IContentRepository data,
            IAuthenticationRepository auth,
            ISubscriptionRepository subs,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IBillingService billing,
            IHoodCache cache,
            ISiteConfiguration site)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _email = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _data = data;
            _auth = auth;
            _subs = subs;
            _site = site;
            _cache = cache;
            _billing = billing;
            _info = _site.GetBasicSettings();
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
            model.User = _auth.GetUserById(_userManager.GetUserId(User));
            model.Plans = await _subs.GetLevels();
            model.Addons = await _subs.GetAddons();
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
                await _subs.CreateUserSubscription(model.PlanId, model.StripeToken, model.CardId);
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
            model.Plans = await _subs.GetLevels();
            model.Addons = await _subs.GetAddons();
            model.Customer = await _subs.GetCustomerObject(account.User.StripeId, true);
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
                await _subs.UpgradeUserSubscription(id, plan);
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
                await _subs.CancelUserSubscription(id);
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
                await _subs.RemoveUserSubscription(id);
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
                await _subs.ReactivateUserSubscription(id);
                return RedirectToAction("Index", "Billing", new { message = BillingMessage.SubscriptionReactivated });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Billing", new { message = ex.Message.ToEnum(BillingMessage.Error) });
            }
        }

        [HttpPost]
        [Route("stripe/webhooks/")]
        [AllowAnonymous]
        public async Task<StatusCodeResult> WebHooks()
        {
            StringBuilder sw = new StringBuilder();
            var info = _site.GetBasicSettings();
            sw.AppendLine(FormatLog("Webhook recieved on site: " + _info.SiteTitle));
            string url = ControllerContext.HttpContext.GetSiteUrl(true, true);
            sw.AppendLine(FormatLog("Url: " + url));
            var json = new StreamReader(Request.Body).ReadToEnd();
            sw.AppendLine(FormatLog("Processed JSON: "));
            sw.AppendLine(json.ToFormattedJson() + Environment.NewLine);
            var stripeEvent = StripeEventUtility.ParseEvent(json);
            string eventName = stripeEvent != null ? stripeEvent.Type != null ? stripeEvent.Type : "invalid.event.object" : "invalid.event.object";
            sw.AppendLine(FormatLog("Stripe Event detected: " + eventName));
            try
            {
                #region All Event types
                // All the event types explained here: https://stripe.com/docs/api#event_types
                switch (eventName)
                {
                    case "account.updated":
                        //Occurs whenever an account status or property has changed.
                        break;
                    case "account.application.deauthorized":
                        // Occurs whenever a user deauthorizes an application. Sent to the related application only.
                        break;
                    case "application_fee.created":
                        // Occurs whenever an application fee is created on a charge.
                        break;
                    case "application_fee.refunded":
                        // Occurs whenever an application fee is refunded, whether from refunding a charge or from refunding the application fee directly, including partial refunds.
                        break;
                    case "balance.available":
                        // Occurs whenever your Stripe balance has been updated (e.g. when a charge collected is available to be paid out). 
                        // By default, Stripe will automatically transfer any funds in your balance to your bank account on a daily basis.
                        break;
                    case "charge.succeeded":
                        break;
                    case "charge.failed":
                        break;
                    case "charge.refunded":
                        // Occurs whenever a charge is refunded, including partial refunds.
                        break;
                    case "charge.captured":
                        // Occurs whenever a previously uncaptured charge is captured.
                        break;
                    case "charge.updated":
                        // Occurs whenever a charge description or metadata is updated.
                        break;
                    case "charge.dispute.created":
                        // Occurs whenever a customer disputes a charge with their bank (chargeback).
                        break;
                    case "charge.dispute.updated":
                        // Occurs when the dispute is updated (usually with evidence).
                        break;
                    case "charge.dispute.closed":
                        // Occurs when the dispute is resolved and the dispute status changes to won or lost.
                        break;
                    case "customer.created":
                        // Occurs whenever a new customer is created.
                        break;
                    case "customer.updated":
                        // Occurs whenever any property of a customer changes.
                        break;
                    case "customer.deleted":
                        // Occurs whenever a customer is deleted.
                        sw.AppendLine(FormatLog("[Customer Deleted] processing..."));
                        StripeCustomer deletedCustomer = Stripe.Mapper<StripeCustomer>.MapFromJson(stripeEvent.Data.Object.ToString());
                        sw.AppendLine(FormatLog("Customer Object:"));
                        sw.AppendLine(JsonConvert.SerializeObject(deletedCustomer).ToFormattedJson() + Environment.NewLine);
                        var dcUser = await _auth.GetUserByStripeId(deletedCustomer.Id);
                        if (dcUser != null)
                        {
                            sw.AppendLine(FormatLog("Customer Object:"));
                            sw.AppendLine(JsonConvert.SerializeObject(dcUser).ToFormattedJson() + Environment.NewLine);
                            dcUser.StripeId = null;
                            _auth.UpdateUser(dcUser);
                            sw.AppendLine(FormatLog("Stripe Id removed from customer."));
                            sw.AppendLine(FormatLog("[Customer Deleted] complete!"));
                        }
                        else
                        {
                            sw.AppendLine(FormatLog("Could not load customer from id: " + deletedCustomer.Id));
                        }
                        break;
                    case "customer.card.created":
                        // Occurs whenever a new card is created for the customer.
                        break;
                    case "customer.card.updated":
                        // Occurs whenever a card's details are changed.
                        // TODO: Save card updated, might happen when the card is close to expire
                        break;
                    case "customer.card.deleted":
                        // Occurs whenever a card is removed from a customer.
                        break;
                    case "customer.subscription.created":
                        // Occurs whenever a customer with no subscription is signed up for a plan.
                        sw.AppendLine(FormatLog("[Subscription Created] processing..."));
                        StripeSubscription created = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
                        sw.AppendLine(FormatLog("Subscription Object:"));
                        sw.AppendLine(JsonConvert.SerializeObject(created).ToFormattedJson() + Environment.NewLine);
                        await _subs.ConfirmSubscriptionObject(created, stripeEvent.Created);
                        sw.AppendLine(FormatLog("[Subscription Created] complete!"));
                        break;
                    case "customer.subscription.updated":
                        // Occurs whenever a subscription changes. Examples would include switching from one plan to another, or switching status from trial to active.
                        sw.AppendLine(FormatLog("[Subscription Updated] processing..."));
                        StripeSubscription updated = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
                        sw.AppendLine(FormatLog("Subscription Object:"));
                        sw.AppendLine(JsonConvert.SerializeObject(updated).ToFormattedJson() + Environment.NewLine);
                        await _subs.UpdateSubscriptionObject(updated, stripeEvent.Created);
                        sw.AppendLine(FormatLog("[Subscription Created] complete!"));
                        break;
                    case "customer.subscription.deleted":
                        // Occurs whenever a customer ends their subscription.
                        sw.AppendLine(FormatLog("[Subscription Deleted] processing..."));
                        StripeSubscription deleted = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
                        sw.AppendLine(FormatLog("Subscription Object:"));
                        sw.AppendLine(JsonConvert.SerializeObject(deleted).ToFormattedJson() + Environment.NewLine);
                        await _subs.RemoveUserSubscriptionObject(deleted, stripeEvent.Created);
                        sw.AppendLine(FormatLog("[Subscription Deleted] complete!"));
                        break;
                    case "customer.subscription.trial_will_end":
                        // Occurs three days before the trial period of a subscription is scheduled to end.
                        sw.AppendLine(FormatLog("[Subscription TrialWillEnd] processing..."));
                        StripeSubscription endTrialSubscription = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
                        sw.AppendLine(FormatLog("Subscription Object:"));
                        sw.AppendLine(JsonConvert.SerializeObject(endTrialSubscription).ToFormattedJson() + Environment.NewLine);
                        UserSubscription endTrialUserSub = await _subs.FindUserSubscriptionByStripeId(endTrialSubscription.Id);
                        if (endTrialUserSub != null)
                        {
                            sw.AppendLine(FormatLog("Local User Subscription Object:"));
                            sw.AppendLine(JsonConvert.SerializeObject(endTrialUserSub).ToFormattedJson() + Environment.NewLine);
                            sw.AppendLine(FormatLog("Send an email to the customer letting them know what has happened..."));

                            MailObject message = new MailObject();
                            message.To = new SendGrid.Helpers.Mail.EmailAddress(endTrialUserSub.User.Email);
                            message.PreHeader = "Your trial will soon expire on " + _site.GetSiteTitle();
                            message.Subject = "Your trial will soon expire...";
                            message.AddH1("The end is near!");
                            message.AddParagraph("Your trial subscription will soon run out.");
                            message.AddParagraph(endTrialSubscription.TrialEnd.Value.ToShortDateString() + " " + endTrialSubscription.TrialEnd.Value.ToShortTimeString());
                            message.AddParagraph("If you have not added a card to your account, please log in and do so now in order to continue using the service.");
                            message.AddParagraph("Alternatively, if you no longer wish to continue using the service, please log in and cancel your subscription to ensure you do not get charged when the trial runs out.");
                            await _email.SendEmail(message);

                            sw.AppendLine(FormatLog("Sent email to customer: " + endTrialUserSub.User.Email));
                        }
                        else
                        {
                            sw.AppendLine(FormatLog("There is no subscription, so ensure that there is no subscription left on stripe."));
                            await _billing.Subscriptions.CancelSubscriptionAsync(endTrialSubscription.CustomerId, endTrialSubscription.Id, false);
                            sw.AppendLine(FormatLog("Send an email to the customer letting them know what has happened..."));
                            var endTrialUser = await _auth.GetUserByStripeId(endTrialSubscription.CustomerId);

                            MailObject message = new MailObject();
                            message.To = new SendGrid.Helpers.Mail.EmailAddress(endTrialUser.Email);
                            message.PreHeader = "Error with your subscription on " + _site.GetSiteTitle();
                            message.Subject = "Error with your subscription...";
                            message.AddH1("Oops!");
                            message.AddParagraph("Seems there was an error with your subscription.");
                            message.AddParagraph("The charge has failed, and we couldn't find a valid subscription on the service, therefore we have prevented any further charges to your card.");
                            await _email.SendEmail(message, MailSettings.DangerTemplate);

                            sw.AppendLine(FormatLog("Sent email to customer: " + endTrialUser.Email));
                        }
                        sw.AppendLine(FormatLog("[Subscription TrialWillEnd] complete!"));
                        break;
                    case "customer.discount.created":
                        // Occurs whenever a coupon is attached to a customer.
                        break;
                    case "customer.discount.updated":
                        // Occurs whenever a customer is switched from one coupon to another.
                        break;
                    case "customer.discount.deleted":
                        break;
                    case "invoice.created":
                        break;
                    case "invoice.payment_failed":
                        // Occurs whenever an invoice attempts to be paid, and the payment fails. 
                        // This can occur either due to a declined payment, or because the customer has no active card.
                        // A particular case of note is that if a customer with no active card reaches the end of its free trial, an invoice.payment_failed notification will occur.
                        sw.AppendLine(FormatLog("[Invoice PaymentFailed] processing..."));
                        StripeInvoice failedInvoice = Stripe.Mapper<StripeInvoice>.MapFromJson(stripeEvent.Data.Object.ToString());
                        sw.AppendLine(FormatLog("StripeInvoice Object:"));
                        sw.AppendLine(JsonConvert.SerializeObject(failedInvoice).ToFormattedJson() + Environment.NewLine);
                        if (failedInvoice.SubscriptionId.IsSet())
                        {
                            // Get the subscription.
                            StripeSubscription failedInvoiceSubscription = await _billing.Subscriptions.FindById(failedInvoice.CustomerId, failedInvoice.SubscriptionId);
                            sw.AppendLine(FormatLog("StripeSubscription Object:"));
                            sw.AppendLine(JsonConvert.SerializeObject(failedInvoiceSubscription).ToFormattedJson() + Environment.NewLine);

                            UserSubscription failedInvoiceUserSub = await _subs.FindUserSubscriptionByStripeId(failedInvoiceSubscription.Id);
                            if (failedInvoiceUserSub != null)
                            {
                                sw.AppendLine(FormatLog("Local User Subscription Object:"));
                                sw.AppendLine(JsonConvert.SerializeObject(failedInvoiceUserSub).ToFormattedJson() + Environment.NewLine);
                                sw.AppendLine(FormatLog("Send an email to the customer letting them know what has happened..."));

                                MailObject message = new MailObject();
                                message.To = new SendGrid.Helpers.Mail.EmailAddress(failedInvoiceUserSub.User.Email);
                                message.PreHeader = "Error with your subscription on " + _site.GetSiteTitle();
                                message.Subject = "Error with your subscription...";
                                message.AddH1("Oops!");
                                message.AddParagraph("Seems there was an error with your subscription.");
                                message.AddParagraph("The charge has failed, this could be due to an expired card or other issue, please check your account and update your payment information to continue using the service.");
                                await _email.SendEmail(message, MailSettings.DangerTemplate);

                            }
                            else
                            {
                                sw.AppendLine(FormatLog("There is no subscription, so ensure that there is no subscription left on stripe."));
                                sw.AppendLine(FormatLog("Cancelling subscription..."));
                                await _billing.Subscriptions.CancelSubscriptionAsync(failedInvoice.CustomerId, failedInvoice.SubscriptionId, false);
                                sw.AppendLine(FormatLog("Send an email to the customer letting them know what has happened..."));
                                var failedInvoiceUser = await _auth.GetUserByStripeId(failedInvoice.CustomerId);

                                MailObject message = new MailObject();
                                message.To = new SendGrid.Helpers.Mail.EmailAddress(failedInvoiceUser.Email);
                                message.PreHeader = "Error with your subscription on " + _site.GetSiteTitle();
                                message.Subject = "Error with your subscription...";
                                message.AddH1("Oops!");
                                message.AddParagraph("Seems there was an error with your subscription.");
                                message.AddParagraph("The charge has failed, this could be due to an expired card or other issue, please check your account and update your payment information to continue using the service.");
                                await _email.SendEmail(message, MailSettings.DangerTemplate);
                            }
                        }
                        // TODO: Notify customer
                        break;
                    case "invoice.payment_succeeded":
                        // Occurs whenever an invoice attempts to be paid, and the payment succeeds.
                        sw.AppendLine(FormatLog("[Invoice PaymentSucceeded] processing..."));
                        StripeInvoice successfulInvoice = Stripe.Mapper<StripeInvoice>.MapFromJson(stripeEvent.Data.Object.ToString());
                        sw.AppendLine(FormatLog("StripeInvoice Object:"));
                        sw.AppendLine(JsonConvert.SerializeObject(successfulInvoice).ToFormattedJson() + Environment.NewLine);

                        if (successfulInvoice.SubscriptionId.IsSet())
                        {
                            // Get the subscription.
                            StripeSubscription subscription = await _billing.Subscriptions.FindById(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId);
                            sw.AppendLine(FormatLog("StripeSubscription Object:"));
                            sw.AppendLine(JsonConvert.SerializeObject(subscription).ToFormattedJson() + Environment.NewLine);

                            UserSubscription userSub = await _subs.FindUserSubscriptionByStripeId(subscription.Id);
                            // if ths sub is set up in the db ALL IS WELL. Continuer
                            if (userSub == null)
                            {

                                sw.AppendLine(FormatLog("The subscription is NOT in the db we must roll back the charge and subscription on stripe."));

                                StripeRefund refund = await _billing.Stripe.RefundService.CreateAsync(successfulInvoice.ChargeId);
                                sw.AppendLine(FormatLog("StripeRefund Object Created:"));
                                sw.AppendLine(JsonConvert.SerializeObject(refund).ToFormattedJson() + Environment.NewLine);

                                await _billing.Subscriptions.CancelSubscriptionAsync(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId, false);
                                sw.AppendLine(FormatLog("Subscription Cancelled."));

                                sw.AppendLine(FormatLog("Send an email to the customer letting them know what has happened..."));
                                var successfulInvoiceUser = await _auth.GetUserByStripeId(successfulInvoice.CustomerId);

                                MailObject message = new MailObject();
                                message.To = new SendGrid.Helpers.Mail.EmailAddress(successfulInvoiceUser.Email);
                                message.PreHeader = "Error with your subscription on " + _site.GetSiteTitle();
                                message.Subject = "Error with your subscription...";
                                message.AddH1("Oops!");
                                message.AddParagraph("Seems there was an error with your subscription.");
                                message.AddParagraph("The charge was created, and completed however the subscription could not be created. Therefore we have refunded the charge to your card, and reset the subscription. Please subscribe again in order to reinstate your subscription.");
                                await _email.SendEmail(message, MailSettings.DangerTemplate);

                            }
                            else
                            {
                                sw.AppendLine(FormatLog("Local User Subscription Object:"));
                                sw.AppendLine(JsonConvert.SerializeObject(userSub).ToFormattedJson() + Environment.NewLine);

                                sw.AppendLine(FormatLog("Send an email to the customer letting them know they have been charged..."));
                                var successfulInvoiceUser = await _auth.GetUserByStripeId(successfulInvoice.CustomerId);

                                MailObject message = new MailObject();
                                message.To = new SendGrid.Helpers.Mail.EmailAddress(successfulInvoiceUser.Email);
                                message.PreHeader = "Thank you for your payment on " + _site.GetSiteTitle();
                                message.Subject = "Thank you for your payment...";
                                message.AddH1("Thank you!");
                                message.AddParagraph("Your payment has been received. You will recieve a payment reciept from our payment providers, Stripe.");
                                await _email.SendEmail(message, MailSettings.DangerTemplate);
                            }
                        }
                        break;
                    case "invoice.updated":
                        // Occurs whenever an invoice changes (for example, the amount could change).
                        break;
                    case "invoiceitem.created":
                        // Occurs whenever an invoice item is created.
                        break;
                    case "invoiceitem.updated":
                        // Occurs whenever an invoice item is updated.
                        break;
                    case "invoiceitem.deleted":
                        // Occurs whenever an invoice item is deleted.
                        break;
                    case "plan.created":
                        // Occurs whenever a plan is created.
                        break;
                    case "plan.updated":
                        // Occurs whenever a plan is updated.
                        break;
                    case "plan.deleted":
                        // Occurs whenever a plan is deleted.
                        break;
                    case "coupon.created":
                        // Occurs whenever a coupon is created.
                        break;
                    case "coupon.deleted":
                        // Occurs whenever a coupon is deleted.
                        break;
                    case "transfer.created":
                        // Occurs whenever a new transfer is created.
                        break;
                    case "transfer.updated":
                        // Occurs whenever the description or metadata of a transfer is updated.
                        break;
                    case "transfer.paid":
                        // Occurs whenever a sent transfer is expected to be available in the destination bank account. 
                        // If the transfer failed, a transfer.failed webhook will additionally be sent at a later time.
                        break;
                    case "transfer.failed":
                        // Occurs whenever Stripe attempts to send a transfer and that transfer fails.
                        break;
                }
                #endregion
                BillingSettings settings = _site.GetBillingSettings();
                switch (settings.SubscriptionWebhookLogs)
                {
                    case "email":

                        MailObject message = new MailObject();
                        message.To = new SendGrid.Helpers.Mail.EmailAddress(info.Email);
                        message.PreHeader = "You access information for " + _site.GetSiteTitle();
                        message.Subject = "You account has been created.";
                        message.AddH1("Stripe Error!");
                        message.AddParagraph(string.Format("Stripe WebHook failed: {0} - {1}", url, eventName));
                        await _email.SendEmail(message);

                        break;
                }
                return new StatusCodeResult(202);
            }
            catch (Exception ex)
            {
                sw.AppendLine(FormatLog("AN EXCEPTION HAS BEEN CAUGHT!"));
                sw.AppendLine(FormatLog("Message: " + ex.Message));
                sw.AppendLine(FormatLog("Stack Trace: " + ex.StackTrace));
                sw.AppendLine(FormatLog("Source: " + ex.Source));
                if (ex.InnerException != null)
                {
                    sw.AppendLine(FormatLog("Inner Message: " + ex.InnerException.Message));
                    sw.AppendLine(FormatLog("Inner Stack Trace: " + ex.InnerException.StackTrace));
                    sw.AppendLine(FormatLog("Inner Source: " + ex.InnerException.Source));
                }

                BillingSettings settings = _site.GetBillingSettings();
                switch (settings.SubscriptionWebhookLogs)
                {
                    case "email":
                    case "email-failures":

                        MailObject message = new MailObject();
                        message.To = new SendGrid.Helpers.Mail.EmailAddress(info.Email);
                        message.PreHeader = "You access information for " + _site.GetSiteTitle();
                        message.Subject = "You account has been created.";
                        message.AddH1("Stripe Error!");
                        message.AddParagraph(string.Format("Stripe WebHook failed: {0} - {1}", url, eventName));
                        await _email.SendEmail(message);
                        break;
                }
                return new StatusCodeResult(500);
            }
        }
        private string FormatLog(string v)
        {
            return DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + ": " + v + Environment.NewLine;
        }
    }

}


