using Hood.Events;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class StripeWebHookService : IStripeWebHookService
    {
        private readonly IAccountRepository _auth;
        private readonly IBillingService _billing;
        private readonly ISettingsRepository _settings;
        private readonly HttpContext _context;
        private readonly IEmailSender _email;

        private readonly BasicSettings _basicSettings;
        private readonly BillingSettings _billingSettings;
        private readonly MailSettings _mailSettings;

        private readonly EventsService _events;

        private MailObject _log;

        public StripeWebHookService(
            IAccountRepository auth,
            ISettingsRepository site,
            IBillingService billing,
            IEmailSender email,
            IHttpContextAccessor contextAccessor,
            EventsService events)
        {
            _auth = auth;
            _settings = site;
            _basicSettings = site.GetBasicSettings();
            _mailSettings = site.GetMailSettings();
            _billingSettings = site.GetBillingSettings();
            _billing = billing;
            _context = contextAccessor.HttpContext;
            _email = email;
            _events = events;
            var info = _settings.GetBasicSettings();
            _log = new MailObject()
            {
                PreHeader = "A new stripe webhook has been fired."
            };
            _log.Subject = _log.PreHeader;
            _log.AddH1("Stripe Webhook Recieved!");
            _log.AddParagraph("Webhook recieved on site: <strong>" + info.SiteTitle + "</strong>");
            _log.AddParagraph("Url: <strong>" + _context.GetSiteUrl() + "</strong>");
            _log.AddH2("Log:");
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

                _log.AddParagraph("Stripe Event detected: <strong>" + args.StripeEvent.GetEventName() + "</strong>");
                _log.AddParagraph("Processed JSON: <strong>" + args.Json.ToFormattedJson() + "</strong>");

                if (stripeEvent.GetEventName() == "invalid.event.object")
                    throw new Exception("The event object was invalid.");

                this.ProcessEventByType(args.StripeEvent);

                switch (_billingSettings.SubscriptionWebhookLogs)
                {
                    case "email":
                        _email.NotifyRoleAsync(_log, "SuperUser", MailSettings.SuccessTemplate).GetAwaiter().GetResult();
                        break;
                }

                // Fire the event to allow any other packages to process the webhook.
                _events.triggerStripeWebhook(this, args);
            }
            catch (Exception ex)
            {
                _log.PreHeader = "An error occurred processing a stripe webhook.";
                _log.Subject = _log.PreHeader;
                _email.NotifyRoleAsync(_log, "SuperUser", MailSettings.DangerTemplate).GetAwaiter().GetResult();

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
            _log.AddParagraph("[Customer Deleted] processing...");
            StripeCustomer deletedCustomer = Stripe.Mapper<StripeCustomer>.MapFromJson(stripeEvent.Data.Object.ToString());
            _log.AddParagraph("Customer Object:");
            _log.AddParagraph(JsonConvert.SerializeObject(deletedCustomer).ToFormattedJson() + Environment.NewLine);
            var dcUser = _auth.GetUserByStripeId(deletedCustomer.Id).Result;
            if (dcUser != null)
            {
                _log.AddParagraph("Customer Object:");
                _log.AddParagraph(JsonConvert.SerializeObject(dcUser).ToFormattedJson() + Environment.NewLine);
                dcUser.StripeId = null;
                _auth.UpdateUser(dcUser);
                _log.AddParagraph("Stripe Id removed from customer.");
                _log.AddParagraph("[Customer Deleted] complete!");
            }
            else
            {
                _log.AddParagraph("Could not load customer from id: " + deletedCustomer.Id);
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
            _log.AddParagraph("[Invoice PaymentFailed] processing...");
            StripeInvoice failedInvoice = Stripe.Mapper<StripeInvoice>.MapFromJson(stripeEvent.Data.Object.ToString());
            _log.AddParagraph("StripeInvoice Object:");
            _log.AddParagraph(JsonConvert.SerializeObject(failedInvoice).ToFormattedJson() + Environment.NewLine);
            if (failedInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                StripeSubscription failedInvoiceSubscription = _billing.Subscriptions.FindById(failedInvoice.CustomerId, failedInvoice.SubscriptionId).Result;
                _log.AddParagraph("StripeSubscription Object:");
                _log.AddParagraph(JsonConvert.SerializeObject(failedInvoiceSubscription).ToFormattedJson() + Environment.NewLine);

                UserSubscription failedInvoiceUserSub = _auth.FindUserSubscriptionByStripeId(failedInvoiceSubscription.Id);
                if (failedInvoiceUserSub != null)
                {
                    _log.AddParagraph("Local User Subscription Object:");
                    _log.AddParagraph(JsonConvert.SerializeObject(failedInvoiceUserSub).ToFormattedJson() + Environment.NewLine);
                    _log.AddParagraph("Send an email to the customer letting them know what has happened...");

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
                    _log.AddParagraph("There is no subscription, so ensure that there is no subscription left on stripe.");
                    _log.AddParagraph("Cancelling subscription...");
                    _billing.Subscriptions.CancelSubscriptionAsync(failedInvoice.CustomerId, failedInvoice.SubscriptionId, false).GetAwaiter().GetResult();
                    _log.AddParagraph("Send an email to the customer letting them know what has happened...");
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
            _log.AddParagraph("[Invoice PaymentSucceeded] processing...");
            StripeInvoice successfulInvoice = Stripe.Mapper<StripeInvoice>.MapFromJson(stripeEvent.Data.Object.ToString());
            _log.AddH3("StripeInvoice Object:");
            _log.AddParagraph(JsonConvert.SerializeObject(successfulInvoice).ToFormattedJson() + Environment.NewLine);

            if (successfulInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                StripeSubscription subscription = _billing.Subscriptions.FindById(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId).Result;
                _log.AddParagraph("StripeSubscription Object:");
                _log.AddParagraph(JsonConvert.SerializeObject(subscription).ToFormattedJson() + Environment.NewLine);

                UserSubscription userSub = _auth.FindUserSubscriptionByStripeId(subscription.Id);
                // if ths sub is set up in the db ALL IS WELL. Continuer
                if (userSub == null)
                {

                    _log.AddParagraph("The subscription is NOT in the db we must roll back the charge and subscription on stripe.");

                    StripeRefund refund = _billing.Stripe.RefundService.CreateAsync(successfulInvoice.ChargeId).Result;
                    _log.AddParagraph("StripeRefund Object Created:");
                    _log.AddParagraph(JsonConvert.SerializeObject(refund).ToFormattedJson() + Environment.NewLine);

                    _billing.Subscriptions.CancelSubscriptionAsync(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId, false).GetAwaiter().GetResult();
                    _log.AddParagraph("Subscription Cancelled.");

                    _log.AddParagraph("Send an email to the customer letting them know what has happened...");
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
                    _log.AddParagraph("Local User Subscription Object:");
                    _log.AddParagraph(JsonConvert.SerializeObject(userSub).ToFormattedJson() + Environment.NewLine);

                    _log.AddParagraph("Send an email to the customer letting them know they have been charged...");
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
            _log.AddParagraph("[Subscription Created] processing...");
            StripeSubscription created = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            var log = _auth.ConfirmSubscriptionObject(created, stripeEvent.Created);
            _log.AddParagraph(log.Replace(Environment.NewLine, "<br />"));
            _log.AddParagraph("[Subscription Created] complete!");
        }
        /// <summary>
        /// Occurs whenever a subscription changes. Examples would include switching from one plan to another, or switching status from trial to active.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionUpdated(StripeEvent stripeEvent)
        {
            _log.AddParagraph("[Subscription Updated] processing...");
            StripeSubscription updated = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            var log = _auth.UpdateSubscriptionObject(updated, stripeEvent.Created);
            _log.AddParagraph(log.Replace(Environment.NewLine, "<br />"));
            _log.AddParagraph("[Subscription Updated] complete!");
        }
        /// <summary>
        /// Occurs whenever a customer ends their subscription.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionDeleted(StripeEvent stripeEvent)
        {
            _log.AddParagraph("[Subscription Deleted] processing...");
            StripeSubscription deleted = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            _log.AddH2("Log:");
            var log = _auth.RemoveUserSubscriptionObject(deleted, stripeEvent.Created);
            _log.AddParagraph("[Subscription Deleted] complete!");
        }
        /// <summary>
        /// Occurs three days before the trial period of a subscription is scheduled to end.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionTrialWillEnd(StripeEvent stripeEvent)
        {
            _log.AddParagraph("[Subscription TrialWillEnd] processing...");
            StripeSubscription endTrialSubscription = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            UserSubscription endTrialUserSub = _auth.FindUserSubscriptionByStripeId(endTrialSubscription.Id);
            if (endTrialUserSub != null)
            {
                _log.AddParagraph("Local User Subscription Object:");
                _log.AddParagraph(JsonConvert.SerializeObject(endTrialUserSub).ToFormattedJson() + Environment.NewLine);
                _log.AddParagraph("Send an email to the customer letting them know what has happened...");

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

                _log.AddParagraph("Sent email to customer: " + endTrialUserSub.User.Email);
            }
            else
            {
                _log.AddParagraph("There is no subscription, so ensure that there is no subscription left on stripe.");
                _billing.Subscriptions.CancelSubscriptionAsync(endTrialSubscription.CustomerId, endTrialSubscription.Id, false).GetAwaiter().GetResult();
                _log.AddParagraph("Send an email to the customer letting them know what has happened...");
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

                _log.AddParagraph("Sent email to customer: " + endTrialUser.Email);
            }
            _log.AddParagraph("[Subscription TrialWillEnd] complete!");
        }
        /// <summary>
        /// Called whenever an unknown webhook arrives from stripe.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void UnhandledWebHook(StripeEvent stripeEvent)
        {
            _log.AddParagraph("The event name could not resolve to a web hook handler: " + stripeEvent.GetEventName());
        }
    }
}
