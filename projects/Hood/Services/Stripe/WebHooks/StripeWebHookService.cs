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
            _auth = Engine.Services.Resolve<IAccountRepository>();

            _basicSettings = Engine.Settings.Basic;
            _mailSettings = Engine.Settings.Mail;
            _billingSettings = Engine.Settings.Billing;
            _context = contextAccessor.HttpContext;
            var info = Engine.Settings.Basic;
            _mailObject = new MailObject()
            {
                PreHeader = "A new stripe webhook has been fired."
            };
            _mailObject.Subject = _mailObject.PreHeader;
            _mailObject.AddH1("Stripe Webhook Recieved!");
            _mailObject.AddParagraph("Webhook recieved on site: <strong>" + info.FullTitle + "</strong>");
            _mailObject.AddParagraph("Url: <strong>" + _context.GetSiteUrl() + "</strong>");
            _mailObject.AddH2("Log:");
        }

        public void ProcessEvent(string json)
        {
            var stripeEvent = Stripe.EventUtility.ParseEvent(json);
            ProcessEvent(stripeEvent);
        }
        public void ProcessEvent(Stripe.Event stripeEvent)
        {
            try
            {
                var args = new StripeWebHookTriggerArgs(stripeEvent);

                _mailObject.AddParagraph("Stripe Event detected: <strong>" + stripeEvent.GetEventName() + "</strong>");

                if (stripeEvent.GetEventName() == "invalid.event.object")
                    throw new Exception("The event object was invalid.");

                this.ProcessEventByType(stripeEvent);

                switch (_billingSettings.SubscriptionWebhookLogs)
                {
                    case "email":
                        _mailObject.Template = MailSettings.SuccessTemplate;
                        _email.NotifyRoleAsync(_mailObject, "SuperUser").GetAwaiter().GetResult();
                        break;
                }

                if (!_env.IsProduction())
                {
                    _logService.AddLogAsync<StripeWebHookService>("Stripe webhook processed.", JsonConvert.SerializeObject(new { Event = stripeEvent }), Models.LogType.Success);
                }

                // Fire the event to allow any other packages to process the webhook.
                _eventService.TriggerStripeWebhook(this, args);
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync<StripeWebHookService>("An error occurred processing a stripe webhook.", new { Message = ex.Message, Event = stripeEvent, Exception = ex }, Models.LogType.Error);

                _mailObject.PreHeader = "An error occurred processing a stripe webhook.";
                _mailObject.Subject = _mailObject.PreHeader;
                _mailObject.Template = MailSettings.DangerTemplate;

                _email.NotifyRoleAsync(_mailObject, "SuperUser").GetAwaiter().GetResult();

                // Throw the error back to the application, for creating the response object.
                throw ex;
            }
        }


        /// <summary>
        /// Occurs whenever a card is removed from a customer.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerCardDeleted(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a card's details are changed.
        /// TODO: Save card updated, might happen when the card is close to expire
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerCardUpdated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a new card is created for the customer.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerCardCreated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever any property of a customer changes.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerUpdated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a new customer is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerCreated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a charge description or metadata is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeUpdated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a previously uncaptured charge is captured.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeCaptured(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a charge is refunded, including partial refunds.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeRefunded(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a failed charge attempt occurs.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeFailed(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a new charge is created and is successful.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void ChargeSucceeded(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a customer is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void CustomerDeleted(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Customer Deleted] processing...");
            Stripe.Customer deletedCustomer = Stripe.Mapper<Stripe.Customer>.MapFromJson(stripeEvent.Data.Object.ToString());
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
        public void InvoiceCreated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever an invoice item is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceItemCreated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever an invoice item is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceItemDeleted(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever an invoice item is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceItemUpdated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever an invoice attempts to be paid, and the payment fails. 
        /// This can occur either due to a declined payment, or because the customer has no active card.
        /// A particular case of note is that if a customer with no active card reaches the end of its free trial, an invoice.payment_failed notification will occur.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoicePaymentFailed(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Invoice PaymentFailed] processing...");
            Stripe.Invoice failedInvoice = Stripe.Mapper<Stripe.Invoice>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddParagraph("StripeInvoice Object:");
            _mailObject.AddParagraph(JsonConvert.SerializeObject(failedInvoice).ToFormattedJson() + Environment.NewLine);
            if (failedInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                Stripe.Subscription failedInvoiceSubscription = _billing.Subscriptions.FindById(failedInvoice.CustomerId, failedInvoice.SubscriptionId).Result;
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
                        PreHeader = "Error with your subscription on " + Engine.Settings.Basic.FullTitle,
                        Subject = "Error with your subscription..."
                    };
                    message.AddH1("Oops!");
                    message.AddParagraph("Seems there was an error with your subscription.");
                    message.AddParagraph("The charge has failed, this could be due to an expired card or other issue, please check your account and update your payment information to continue using the service.");
                    message.Template = MailSettings.DangerTemplate;
                    _email.SendEmailAsync(message).GetAwaiter().GetResult();

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
                        PreHeader = "Error with your subscription on " + Engine.Settings.Basic.FullTitle,
                        Subject = "Error with your subscription..."
                    };
                    message.AddH1("Oops!");
                    message.AddParagraph("Seems there was an error with your subscription.");
                    message.AddParagraph("The charge has failed, this could be due to an expired card or other issue, please check your account and update your payment information to continue using the service.");
                    message.Template = MailSettings.DangerTemplate;
                    _email.SendEmailAsync(message).GetAwaiter().GetResult();
                }
            }

        }
        /// <summary>
        /// Occurs whenever an invoice attempts to be paid, and the payment succeeds.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoicePaymentSucceeded(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Invoice PaymentSucceeded] processing...");
            Stripe.Invoice successfulInvoice = Stripe.Mapper<Stripe.Invoice>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddH3("StripeInvoice Object:");
            _mailObject.AddParagraph(JsonConvert.SerializeObject(successfulInvoice).ToFormattedJson() + Environment.NewLine);

            if (successfulInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                Stripe.Subscription subscription = _billing.Subscriptions.FindById(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId).Result;
                _mailObject.AddParagraph($"StripeSubscription Object loaded from Stripe: {successfulInvoice.SubscriptionId}");

                UserSubscription userSub = _auth.FindUserSubscriptionByStripeId(subscription.Id);
                // if ths sub is set up in the db ALL IS WELL. Continuer
                if (userSub == null)
                {

                    _mailObject.AddParagraph("The subscription is NOT in the db we must roll back the charge and subscription on stripe.");

                    Stripe.Refund refund = _billing.Stripe.RefundService.CreateAsync(new RefundCreateOptions { ChargeId = successfulInvoice.ChargeId }).Result;
                    _mailObject.AddParagraph("StripeRefund Object Created.");

                    _billing.Subscriptions.CancelSubscriptionAsync(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId, false).GetAwaiter().GetResult();
                    _mailObject.AddParagraph("Subscription Cancelled.");

                    _mailObject.AddParagraph("Send an email to the customer letting them know what has happened...");
                    var successfulInvoiceUser = _auth.GetUserByStripeId(successfulInvoice.CustomerId).Result;

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(successfulInvoiceUser.Email),
                        PreHeader = "Error with your subscription on " + Engine.Settings.Basic.FullTitle,
                        Subject = "Error with your subscription..."
                    };
                    message.AddH1("Oops!");
                    message.AddParagraph("Seems there was an error with your subscription.");
                    message.AddParagraph("The charge was created, and completed however the subscription could not be created. Therefore we have refunded the charge to your card, and reset the subscription. Please subscribe again in order to reinstate your subscription.");
                    message.Template = MailSettings.DangerTemplate;
                    _email.SendEmailAsync(message).GetAwaiter().GetResult();

                }
                else
                {
                    _mailObject.AddParagraph($"Local User Subscription Object located with Id: {userSub.Id}");
                    _mailObject.AddParagraph("Sending an email to the customer letting them know they have been charged...");
                    var successfulInvoiceUser = _auth.GetUserByStripeId(successfulInvoice.CustomerId).Result;

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(successfulInvoiceUser.Email),
                        PreHeader = "Thank you for your payment on " + Engine.Settings.Basic.FullTitle,
                        Subject = "Thank you for your payment..."
                    };
                    message.AddH1("Thank you!");
                    message.AddParagraph("Your payment has been received. You will recieve a payment reciept from our payment providers, Stripe.");
                    message.Template = MailSettings.DangerTemplate;
                    _email.SendEmailAsync(message).GetAwaiter().GetResult();

                    _mailObject.AddParagraph($"Email sent to customer: {successfulInvoiceUser.Email}");

                }
            }
        }
        /// <summary>
        /// Occurs whenever an invoice is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InvoiceUpdated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a plan is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void PlanDeleted(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a plan is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void PlanUpdated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a plan is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void PlanCreated(Stripe.Event stripeEvent)
        {
            UnhandledWebHook(stripeEvent);
        }
        /// <summary>
        /// Occurs whenever a customer with no subscription is signed up for a plan.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionCreated(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Created] processing...");
            Stripe.Subscription created = Stripe.Mapper<Stripe.Subscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            var log = _auth.ConfirmSubscriptionObject(created, stripeEvent.Created);
            _mailObject.AddParagraph(log.Replace(Environment.NewLine, "<br />"));
            _mailObject.AddParagraph("[Subscription Created] complete!");
        }
        /// <summary>
        /// Occurs whenever a subscription changes. Examples would include switching from one plan to another, or switching status from trial to active.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionUpdated(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Updated] processing...");
            Stripe.Subscription updated = Stripe.Mapper<Stripe.Subscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            var log = _auth.UpdateSubscriptionObject(updated, stripeEvent.Created);
            _mailObject.AddParagraph(log.Replace(Environment.NewLine, "<br />"));
            _mailObject.AddParagraph("[Subscription Updated] complete!");
        }
        /// <summary>
        /// Occurs whenever a customer ends their subscription.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionDeleted(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Deleted] processing...");
            Stripe.Subscription deleted = Stripe.Mapper<Stripe.Subscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddH2("Log:");
            var log = _auth.RemoveUserSubscriptionObject(deleted, stripeEvent.Created);
            _mailObject.AddParagraph("[Subscription Deleted] complete!");
        }
        /// <summary>
        /// Occurs three days before the trial period of a subscription is scheduled to end.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void SubscriptionTrialWillEnd(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription TrialWillEnd] processing...");
            Stripe.Subscription endTrialSubscription = Stripe.Mapper<Stripe.Subscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            UserSubscription endTrialUserSub = _auth.FindUserSubscriptionByStripeId(endTrialSubscription.Id);
            if (endTrialUserSub != null)
            {
                _mailObject.AddParagraph($"Local User Subscription object found: {endTrialUserSub.Id}");
                _mailObject.AddParagraph("Sending an email to the customer letting them know what has happened...");

                MailObject message = new MailObject()
                {
                    To = new SendGrid.Helpers.Mail.EmailAddress(endTrialUserSub.User.Email),
                    PreHeader = "Your trial will soon expire on " + Engine.Settings.Basic.FullTitle,
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
                    PreHeader = "Error with your subscription on " + Engine.Settings.Basic.FullTitle,
                    Subject = "Error with your subscription..."
                };
                message.AddH1("Oops!");
                message.AddParagraph("Seems there was an error with your subscription.");
                message.AddParagraph("The charge has failed, and we couldn't find a valid subscription on the service, therefore we have prevented any further charges to your card.");
                message.Template = MailSettings.DangerTemplate;
                _email.SendEmailAsync(message).GetAwaiter().GetResult();

                _mailObject.AddParagraph("Sent email to customer: " + endTrialUser.Email);
            }
            _mailObject.AddParagraph("[Subscription TrialWillEnd] complete!");
        }
        /// <summary>
        /// Called whenever an unknown webhook arrives from stripe.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void UnhandledWebHook(Stripe.Event stripeEvent)
        {
            _logService.AddLogAsync<StripeWebHookService>("The event name could not resolve to a web hook handler: " + stripeEvent.GetEventName(), stripeEvent, LogType.Warning);
            _mailObject.AddParagraph("The event name could not resolve to a web hook handler: " + stripeEvent.GetEventName());
        }
    }
}
