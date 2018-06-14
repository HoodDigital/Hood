using Hood.Core;
using Hood.Events;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Stripe;
using System;

namespace Hood.Services
{
    public class StripeWebHookService : BaseService<HoodDbContext, ApplicationUser, IdentityRole>, IStripeWebHookService
    {
        protected readonly IAccountRepository _auth;

        protected readonly BasicSettings _basicSettings;
        protected readonly BillingSettings _billingSettings;
        protected readonly MailSettings _mailSettings;
        protected readonly HttpContext _context;

        protected MailObject _mailObject;

        public StripeWebHookService(
            IHttpContextAccessor contextAccessor
            )
            : base()
        {
            _auth = EngineContext.Current.Resolve<IAccountRepository>();

            _basicSettings = _settings.GetBasicSettings();
            _mailSettings = _settings.GetMailSettings();
            _billingSettings = _settings.GetBillingSettings();
            _context = contextAccessor.HttpContext;
            var info = _settings.GetBasicSettings();
            _mailObject = new MailObject()
            {
                PreHeader = "A new stripe webhook has been fired."
            };
            _mailObject.Subject = _mailObject.PreHeader;
            _mailObject.AddH1("Stripe Webhook Recieved!");
            _mailObject.AddParagraph("Webhook recieved on site: <strong>" + info.SiteTitle + "</strong>");
            _mailObject.AddParagraph("Url: <strong>" + _context.GetSiteUrl() + "</strong>");
            _mailObject.AddH2("Log:");
        }

        public void ProcessEvent(string json)
        {
            var stripeEvent = StripeEventUtility.ParseEvent(json);
            ProcessEvent(stripeEvent);
        }
        public void ProcessEvent(StripeEvent stripeEvent)
        {
            try
            {
                var args = new StripeWebHookTriggerArgs(stripeEvent);

                _mailObject.AddParagraph("Stripe Event detected: <strong>" + args.StripeEvent.GetEventName() + "</strong>");

                if (stripeEvent.GetEventName() == "invalid.event.object")
                    throw new Exception("The event object was invalid.");

                this.ProcessEventByType(args.StripeEvent);

                switch (_billingSettings.SubscriptionWebhookLogs)
                {
                    case "email":
                        _email.NotifyRoleAsync(_mailObject, "SuperUser", MailSettings.SuccessTemplate).GetAwaiter().GetResult();
                        break;
                }

                if (!_env.IsProduction())
                {
                    _logService.AddLogAsync("Stripe webhook processed.", _mailObject.Text.ToHtmlLineBreaks(), Models.LogType.Success, Models.LogSource.Subscriptions, null, null, nameof(UserSubscription), null);
                }

                // Fire the event to allow any other packages to process the webhook.
                _eventService.triggerStripeWebhook(this, args);
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync("An error occurred processing a stripe webhook.", JsonConvert.SerializeObject(new { Message = ex.Message, Event = stripeEvent, Exception = ex }), Models.LogType.Error, Models.LogSource.Subscriptions, null, null, nameof(StripeWebHookService), null);

                _mailObject.PreHeader = "An error occurred processing a stripe webhook.";
                _mailObject.Subject = _mailObject.PreHeader;
                _email.NotifyRoleAsync(_mailObject, "SuperUser", MailSettings.DangerTemplate).GetAwaiter().GetResult();

                // Throw the error back to the application, for creating the response object.
                throw ex;
            }
        }


        /// <summary>
        /// Occurs whenever a card is removed from a customer.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerCardDeleted(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a card's details are changed.
        /// TODO: Save card updated, might happen when the card is close to expire
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerCardUpdated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a new card is created for the customer.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerCardCreated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever any property of a customer changes.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerUpdated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a new customer is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerCreated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a charge description or metadata is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeUpdated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a previously uncaptured charge is captured.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeCaptured(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a charge is refunded, including partial refunds.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeRefunded(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a failed charge attempt occurs.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeFailed(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a new charge is created and is successful.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeSucceeded(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a customer is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerDeleted(StripeEvent stripeEvent)
        {
            _mailObject.AddParagraph("[Customer Deleted] processing...");
            StripeCustomer deletedCustomer = Stripe.Mapper<StripeCustomer>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddParagraph("Customer Object:");
            _mailObject.AddParagraph(JsonConvert.SerializeObject(deletedCustomer).ToFormattedJson() + Environment.NewLine);
            var dcUser = _auth.GetUserByStripeId(deletedCustomer.Id).Result;
            if (dcUser != null)
            {
                _mailObject.AddParagraph("Customer Object:");
                _mailObject.AddParagraph(JsonConvert.SerializeObject(dcUser).ToFormattedJson() + Environment.NewLine);
                dcUser.StripeId = null;
                _auth.UpdateUser(dcUser);
                _mailObject.AddParagraph("Stripe Id removed from customer.");
                _mailObject.AddParagraph("[Customer Deleted] complete!");
            }
            else
            {
                _mailObject.AddParagraph("Could not load customer from id: " + deletedCustomer.Id);
            }
        }
        /// <summary>
        /// Occurs whenever an invoice changes (for example, the amount could change).
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceCreated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever an invoice item is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceItemCreated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever an invoice item is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceItemDeleted(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever an invoice item is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceItemUpdated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever an invoice attempts to be paid, and the payment fails. 
        /// This can occur either due to a declined payment, or because the customer has no active card.
        /// A particular case of note is that if a customer with no active card reaches the end of its free trial, an invoice.payment_failed notification will occur.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoicePaymentFailed(StripeEvent stripeEvent)
        {
            _mailObject.AddParagraph("[Invoice PaymentFailed] processing...");
            StripeInvoice failedInvoice = Stripe.Mapper<StripeInvoice>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddParagraph("StripeInvoice Object:");
            _mailObject.AddParagraph(JsonConvert.SerializeObject(failedInvoice).ToFormattedJson() + Environment.NewLine);
            if (failedInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                StripeSubscription failedInvoiceSubscription = _billing.Subscriptions.FindById(failedInvoice.CustomerId, failedInvoice.SubscriptionId).Result;
                _mailObject.AddParagraph("StripeSubscription Object:");
                _mailObject.AddParagraph(JsonConvert.SerializeObject(failedInvoiceSubscription).ToFormattedJson() + Environment.NewLine);

                UserSubscription failedInvoiceUserSub = _auth.FindUserSubscriptionByStripeId(failedInvoiceSubscription.Id);
                if (failedInvoiceUserSub != null)
                {
                    _mailObject.AddParagraph("Local User Subscription Object:");
                    _mailObject.AddParagraph(JsonConvert.SerializeObject(failedInvoiceUserSub).ToFormattedJson() + Environment.NewLine);
                    _mailObject.AddParagraph("Send an email to the customer letting them know what has happened...");

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(failedInvoiceUserSub.User.Email),
                        PreHeader = "Error with your subscription on " + _settings.GetSiteTitle(),
                        Subject = "Error with your subscription..."
                    };
                    message.AddH1("Oops!");
                    message.AddParagraph("Seems there was an error with your subscription.");
                    message.AddParagraph("The charge has failed, this could be due to an expired card or other issue, please check your account and update your payment information to continue using the service.");
                    _email.SendEmailAsync(message, MailSettings.DangerTemplate).GetAwaiter().GetResult();

                }
                else
                {
                    _mailObject.AddParagraph("There is no subscription, so ensure that there is no subscription left on stripe.");
                    _mailObject.AddParagraph("Cancelling subscription...");
                    _billing.Subscriptions.CancelSubscriptionAsync(failedInvoice.CustomerId, failedInvoice.SubscriptionId, false).GetAwaiter().GetResult();
                    _mailObject.AddParagraph("Send an email to the customer letting them know what has happened...");
                    var failedInvoiceUser = _auth.GetUserByStripeId(failedInvoice.CustomerId).Result;

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(failedInvoiceUser.Email),
                        PreHeader = "Error with your subscription on " + _settings.GetSiteTitle(),
                        Subject = "Error with your subscription..."
                    };
                    message.AddH1("Oops!");
                    message.AddParagraph("Seems there was an error with your subscription.");
                    message.AddParagraph("The charge has failed, this could be due to an expired card or other issue, please check your account and update your payment information to continue using the service.");
                    _email.SendEmailAsync(message, MailSettings.DangerTemplate).GetAwaiter().GetResult();
                }
            }

        }
        /// <summary>
        /// Occurs whenever an invoice attempts to be paid, and the payment succeeds.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoicePaymentSucceeded(StripeEvent stripeEvent)
        {
            _mailObject.AddParagraph("[Invoice PaymentSucceeded] processing...");
            StripeInvoice successfulInvoice = Stripe.Mapper<StripeInvoice>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddH3("StripeInvoice Object:");
            _mailObject.AddParagraph(JsonConvert.SerializeObject(successfulInvoice).ToFormattedJson() + Environment.NewLine);

            if (successfulInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                StripeSubscription subscription = _billing.Subscriptions.FindById(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId).Result;
                _mailObject.AddParagraph($"StripeSubscription Object loaded from Stripe: {successfulInvoice.SubscriptionId}");

                UserSubscription userSub = _auth.FindUserSubscriptionByStripeId(subscription.Id);
                // if ths sub is set up in the db ALL IS WELL. Continuer
                if (userSub == null)
                {

                    _mailObject.AddParagraph("The subscription is NOT in the db we must roll back the charge and subscription on stripe.");

                    StripeRefund refund = _billing.Stripe.RefundService.CreateAsync(successfulInvoice.ChargeId).Result;
                    _mailObject.AddParagraph("StripeRefund Object Created.");

                    _billing.Subscriptions.CancelSubscriptionAsync(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId, false).GetAwaiter().GetResult();
                    _mailObject.AddParagraph("Subscription Cancelled.");

                    _mailObject.AddParagraph("Send an email to the customer letting them know what has happened...");
                    var successfulInvoiceUser = _auth.GetUserByStripeId(successfulInvoice.CustomerId).Result;

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(successfulInvoiceUser.Email),
                        PreHeader = "Error with your subscription on " + _settings.GetSiteTitle(),
                        Subject = "Error with your subscription..."
                    };
                    message.AddH1("Oops!");
                    message.AddParagraph("Seems there was an error with your subscription.");
                    message.AddParagraph("The charge was created, and completed however the subscription could not be created. Therefore we have refunded the charge to your card, and reset the subscription. Please subscribe again in order to reinstate your subscription.");
                    _email.SendEmailAsync(message, MailSettings.DangerTemplate).GetAwaiter().GetResult();

                }
                else
                {
                    _mailObject.AddParagraph($"Local User Subscription Object located with Id: {userSub.Id}");
                    _mailObject.AddParagraph("Sending an email to the customer letting them know they have been charged...");
                    var successfulInvoiceUser = _auth.GetUserByStripeId(successfulInvoice.CustomerId).Result;

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(successfulInvoiceUser.Email),
                        PreHeader = "Thank you for your payment on " + _settings.GetSiteTitle(),
                        Subject = "Thank you for your payment..."
                    };
                    message.AddH1("Thank you!");
                    message.AddParagraph("Your payment has been received. You will recieve a payment reciept from our payment providers, Stripe.");
                    _email.SendEmailAsync(message, MailSettings.DangerTemplate).GetAwaiter().GetResult();

                    _mailObject.AddParagraph($"Email sent to customer: {successfulInvoiceUser.Email}");

                }
            }
        }
        /// <summary>
        /// Occurs whenever an invoice is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceUpdated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a plan is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void PlanDeleted(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a plan is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void PlanUpdated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a plan is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void PlanCreated(StripeEvent stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a customer with no subscription is signed up for a plan.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionCreated(StripeEvent stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Created] processing...");
            StripeSubscription created = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            var log = _auth.ConfirmSubscriptionObject(created, stripeEvent.Created);
            _mailObject.AddParagraph(log.Replace(Environment.NewLine, "<br />"));
            _mailObject.AddParagraph("[Subscription Created] complete!");
        }
        /// <summary>
        /// Occurs whenever a subscription changes. Examples would include switching from one plan to another, or switching status from trial to active.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionUpdated(StripeEvent stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Updated] processing...");
            StripeSubscription updated = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            var log = _auth.UpdateSubscriptionObject(updated, stripeEvent.Created);
            _mailObject.AddParagraph(log.Replace(Environment.NewLine, "<br />"));
            _mailObject.AddParagraph("[Subscription Updated] complete!");
        }
        /// <summary>
        /// Occurs whenever a customer ends their subscription.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionDeleted(StripeEvent stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Deleted] processing...");
            StripeSubscription deleted = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddH2("Log:");
            var log = _auth.RemoveUserSubscriptionObject(deleted, stripeEvent.Created);
            _mailObject.AddParagraph("[Subscription Deleted] complete!");
        }
        /// <summary>
        /// Occurs three days before the trial period of a subscription is scheduled to end.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionTrialWillEnd(StripeEvent stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription TrialWillEnd] processing...");
            StripeSubscription endTrialSubscription = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            UserSubscription endTrialUserSub = _auth.FindUserSubscriptionByStripeId(endTrialSubscription.Id);
            if (endTrialUserSub != null)
            {
                _mailObject.AddParagraph($"Local User Subscription object found: {endTrialUserSub.Id}");
                _mailObject.AddParagraph("Sending an email to the customer letting them know what has happened...");

                MailObject message = new MailObject()
                {
                    To = new SendGrid.Helpers.Mail.EmailAddress(endTrialUserSub.User.Email),
                    PreHeader = "Your trial will soon expire on " + _settings.GetSiteTitle(),
                    Subject = "Your trial will soon expire..."
                };
                message.AddH1("The end is near!");
                message.AddParagraph("Your trial subscription will soon run out.");
                message.AddParagraph(endTrialSubscription.TrialEnd.Value.ToShortDateString() + " " + endTrialSubscription.TrialEnd.Value.ToShortTimeString());
                message.AddParagraph("If you have not added a card to your account, please log in and do so now in order to continue using the service.");
                message.AddParagraph("Alternatively, if you no longer wish to continue using the service, please log in and cancel your subscription to ensure you do not get charged when the trial runs out.");
                _email.SendEmailAsync(message).GetAwaiter().GetResult();

                _mailObject.AddParagraph("Sent email to customer: " + endTrialUserSub.User.Email);
            }
            else
            {
                _mailObject.AddParagraph("There is no subscription, so ensure that there is no subscription left on stripe.");
                _billing.Subscriptions.CancelSubscriptionAsync(endTrialSubscription.CustomerId, endTrialSubscription.Id, false).GetAwaiter().GetResult();
                _mailObject.AddParagraph("Send an email to the customer letting them know what has happened...");
                var endTrialUser = _auth.GetUserByStripeId(endTrialSubscription.CustomerId).Result;

                MailObject message = new MailObject()
                {
                    To = new SendGrid.Helpers.Mail.EmailAddress(endTrialUser.Email),
                    PreHeader = "Error with your subscription on " + _settings.GetSiteTitle(),
                    Subject = "Error with your subscription..."
                };
                message.AddH1("Oops!");
                message.AddParagraph("Seems there was an error with your subscription.");
                message.AddParagraph("The charge has failed, and we couldn't find a valid subscription on the service, therefore we have prevented any further charges to your card.");
                _email.SendEmailAsync(message, MailSettings.DangerTemplate).GetAwaiter().GetResult();

                _mailObject.AddParagraph("Sent email to customer: " + endTrialUser.Email);
            }
            _mailObject.AddParagraph("[Subscription TrialWillEnd] complete!");
        }
        /// <summary>
        /// Called whenever an unknown webhook arrives from stripe.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void UnhandledWebHook(StripeEvent stripeEvent)
        {
            if (!_env.IsProduction())
            {
                _logService.AddLogAsync("The event name could not resolve to a web hook handler: " + stripeEvent.GetEventName(), JsonConvert.SerializeObject(stripeEvent), Models.LogType.Info, Models.LogSource.Subscriptions, null, null, nameof(StripeEvent), null);
            }

            _mailObject.AddParagraph("The event name could not resolve to a web hook handler: " + stripeEvent.GetEventName());
        }
    }
}
