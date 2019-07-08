using Hood.Core;
using Hood.Events;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Threading.Tasks;

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

        public StripeWebHookService(IHttpContextAccessor contextAccessor)
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

        public async Task ProcessEventAsync(string eventJson)
        {
            try
            {
                var stripeEvent = EventUtility.ParseEvent(eventJson);

                var args = new StripeWebHookTriggerArgs(stripeEvent);

                _mailObject.AddParagraph("Stripe Event detected: <strong>" + stripeEvent.GetEventName() + "</strong>");

                if (stripeEvent.GetEventName() == "invalid.event.object")
                    throw new Exception("The event object was invalid.");

                await ProcessEventByTypeAsync(stripeEvent);

                switch (_billingSettings.SubscriptionWebhookLogs)
                {
                    case "email":
                        _mailObject.Template = MailSettings.SuccessTemplate;
                        _email.NotifyRoleAsync(_mailObject, "SuperUser").GetAwaiter().GetResult();
                        break;
                }

                // Fire the event to allow any other packages to process the webhook.
                _eventService.TriggerStripeWebhook(this, args);

                await _logService.AddLogAsync<StripeWebHookService>("Stripe webhook processed.", JsonConvert.SerializeObject(new { Event = stripeEvent }), LogType.Success);
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<StripeWebHookService>("An error occurred processing a stripe webhook.", ex);
                _mailObject.PreHeader = "An error occurred processing a stripe webhook.";
                _mailObject.Subject = _mailObject.PreHeader;
                _mailObject.Template = MailSettings.DangerTemplate;
                await _email.NotifyRoleAsync(_mailObject, "Admin");
                throw ex;
            }
        }
        public async Task ProcessEventByTypeAsync(Stripe.Event stripeEvent)
        {
            switch (stripeEvent.GetEventName())
            {
                case "customer.created":
                    await CustomerCreatedAsync(stripeEvent);
                    break;
                case "customer.updated":
                    await CustomerUpdatedAsync(stripeEvent);
                    break;
                case "customer.deleted":
                    await CustomerDeletedAsync(stripeEvent);
                    break;

                case "customer.subscription.created":
                    await SubscriptionCreatedAsync(stripeEvent);
                    break;
                case "customer.subscription.updated":
                    await SubscriptionUpdatedAsync(stripeEvent);
                    break;
                case "customer.subscription.deleted":
                    await SubscriptionDeletedAsync(stripeEvent);
                    break;
                case "customer.subscription.trial_will_end":
                    await SubscriptionTrialWillEndAsync(stripeEvent);
                    break;

                case "invoice.payment_failed":
                    await InvoicePaymentFailedAsync(stripeEvent);
                    break;
                case "invoice.payment_succeeded":
                    await InvoicePaymentSucceededAsync(stripeEvent);
                    break;

                case "plan.created":
                    await PlanCreatedAsync(stripeEvent);
                    break;
                case "plan.updated":
                    await PlanUpdatedAsync(stripeEvent);
                    break;
                case "plan.deleted":
                    await PlanDeletedAsync(stripeEvent);
                    break;

                default:
                    await _logService.AddLogAsync<StripeWebHookService>($"The event name could not resolve to a web hook handler: {stripeEvent.GetEventName()}", stripeEvent, LogType.Warning);
                    break;
            }
        }

        #region Customer Webhooks
        /// <summary>
        /// Occurs whenever a new customer is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task CustomerCreatedAsync(Stripe.Event stripeEvent)
        {
#warning TODO: Sync customer with site on create on Stripe - Make a user if one doesnt exist.
            await Task.Delay(10);
            throw new NotImplementedException();
        }
        /// <summary>
        /// Occurs whenever a new customer is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task CustomerUpdatedAsync(Stripe.Event stripeEvent)
        {
#warning TODO: Sync customer with site on update from Stripe - Make a user if one doesnt exist.
            await Task.Delay(10);
            throw new NotImplementedException();
        }
        /// <summary>
        /// Occurs whenever a customer is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task CustomerDeletedAsync(Stripe.Event stripeEvent)
        {
#warning TODO: Sync customer with site on delete from Stripe.
            _mailObject.AddParagraph("[Customer Deleted] processing...");
            Stripe.Customer deletedCustomer = Mapper<Customer>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddParagraph("Customer Object:");
            _mailObject.AddParagraph(JsonConvert.SerializeObject(deletedCustomer).ToFormattedJson() + Environment.NewLine);
            var dcUser = await _auth.GetUserByStripeIdAsync(deletedCustomer.Id);
            if (dcUser != null)
            {
                _mailObject.AddParagraph("Customer Object:");
                _mailObject.AddParagraph(JsonConvert.SerializeObject(dcUser).ToFormattedJson() + Environment.NewLine);
                dcUser.StripeId = null;
                await _auth.UpdateUserAsync(dcUser);
                _mailObject.AddParagraph("Stripe Id removed from customer.");
                _mailObject.AddParagraph("[Customer Deleted] complete!");
            }
            else
            {
                _mailObject.AddParagraph("Could not load customer from id: " + deletedCustomer.Id);
            }
        }
        #endregion

        #region Invoice Webhooks
        /// <summary>
        /// Occurs whenever an invoice attempts to be paid, and the payment fails. 
        /// This can occur either due to a declined payment, or because the customer has no active card.
        /// A particular case of note is that if a customer with no active card reaches the end of its free trial, an invoice.payment_failed notification will occur.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task InvoicePaymentFailedAsync(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Invoice PaymentFailed] processing...");
            Stripe.Invoice failedInvoice = Mapper<Invoice>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddParagraph("StripeInvoice Object:");
            _mailObject.AddParagraph(JsonConvert.SerializeObject(failedInvoice).ToFormattedJson() + Environment.NewLine);
            if (failedInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                Stripe.Subscription failedInvoiceSubscription = _billing.Subscriptions.FindById(failedInvoice.CustomerId, failedInvoice.SubscriptionId).Result;
                _mailObject.AddParagraph("StripeSubscription Object:");
                _mailObject.AddParagraph(JsonConvert.SerializeObject(failedInvoiceSubscription).ToFormattedJson() + Environment.NewLine);

                UserSubscription failedInvoiceUserSub = await _auth.GetUserSubscriptionByStripeIdAsync(failedInvoiceSubscription.Id);
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
                    message.AddParagraph("The charge has failed, this could be due to an expired card or other issue, please check your account and update your payment information to continue using the");
                    message.Template = MailSettings.DangerTemplate;
                    _email.SendEmailAsync(message).GetAwaiter().GetResult();

                }
                else
                {
                    _mailObject.AddParagraph("There is no subscription, so ensure that there is no subscription left on stripe.");
                    _mailObject.AddParagraph("Cancelling subscription...");
                    _billing.Subscriptions.CancelSubscriptionAsync(failedInvoice.CustomerId, failedInvoice.SubscriptionId, false).GetAwaiter().GetResult();
                    _mailObject.AddParagraph("Send an email to the customer letting them know what has happened...");
                    var failedInvoiceUser = await _auth.GetUserByStripeIdAsync(failedInvoice.CustomerId);

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(failedInvoiceUser.Email),
                        PreHeader = "Error with your subscription on " + Engine.Settings.Basic.FullTitle,
                        Subject = "Error with your subscription..."
                    };
                    message.AddH1("Oops!");
                    message.AddParagraph("Seems there was an error with your subscription.");
                    message.AddParagraph("The charge has failed, this could be due to an expired card or other issue, please check your account and update your payment information to continue using the");
                    message.Template = MailSettings.DangerTemplate;
                    _email.SendEmailAsync(message).GetAwaiter().GetResult();
                }
            }

        }
        /// <summary>
        /// Occurs whenever an invoice attempts to be paid, and the payment succeeds.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task InvoicePaymentSucceededAsync(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Invoice PaymentSucceeded] processing...");
            Stripe.Invoice successfulInvoice = Mapper<Invoice>.MapFromJson(stripeEvent.Data.Object.ToString());
            _mailObject.AddH3("StripeInvoice Object:");
            _mailObject.AddParagraph(JsonConvert.SerializeObject(successfulInvoice).ToFormattedJson() + Environment.NewLine);

            if (successfulInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                Stripe.Subscription subscription = _billing.Subscriptions.FindById(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId).Result;
                _mailObject.AddParagraph($"StripeSubscription Object loaded from Stripe: {successfulInvoice.SubscriptionId}");

                UserSubscription userSub = await _auth.GetUserSubscriptionByStripeIdAsync(subscription.Id);
                // if ths sub is set up in the db ALL IS WELL. Continuer
                if (userSub == null)
                {

                    _mailObject.AddParagraph("The subscription is NOT in the db we must roll back the charge and subscription on stripe.");

                    Stripe.Refund refund = _billing.Stripe.RefundService.CreateAsync(new RefundCreateOptions { ChargeId = successfulInvoice.ChargeId }).Result;
                    _mailObject.AddParagraph("StripeRefund Object Created.");

                    _billing.Subscriptions.CancelSubscriptionAsync(successfulInvoice.CustomerId, successfulInvoice.SubscriptionId, false).GetAwaiter().GetResult();
                    _mailObject.AddParagraph("Subscription Cancelled.");

                    _mailObject.AddParagraph("Send an email to the customer letting them know what has happened...");
                    var successfulInvoiceUser = await _auth.GetUserByStripeIdAsync(successfulInvoice.CustomerId);

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
                    var successfulInvoiceUser = await _auth.GetUserByStripeIdAsync(successfulInvoice.CustomerId);

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
        #endregion

        #region Subscription / Stripe.Plan Webhooks
        /// <summary>
        /// Occurs whenever a plan is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task PlanCreatedAsync(Stripe.Event stripeEvent)
        {
#warning TODO: Sync plan with site on create on Stripe.
            await Task.Delay(10);
            throw new NotImplementedException();
        }
        /// <summary>
        /// Occurs whenever a plan is updated.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task PlanUpdatedAsync(Stripe.Event stripeEvent)
        {
#warning TODO: Sync plan with site on update from Stripe.
            await Task.Delay(10);
            throw new NotImplementedException();
        }
        /// <summary>
        /// Occurs whenever a plan is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task PlanDeletedAsync(Stripe.Event stripeEvent)
        {
#warning TODO: Sync plan with site on delete from Stripe.
            await Task.Delay(10);
            throw new NotImplementedException();
        }
        #endregion

        #region UserSubscription / Stripe.Subscription Webhooks
        /// <summary>
        /// Occurs whenever a customer with no subscription is signed up for a plan.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task SubscriptionCreatedAsync(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Created] processing...");
            Stripe.Subscription created = Mapper<Stripe.Subscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            await _auth.ConfirmSubscriptionObjectAsync(created, stripeEvent.Created);
            _mailObject.AddParagraph("[Subscription Created] complete!");
        }
        /// <summary>
        /// Occurs whenever a subscription changes. Examples would include switching from one plan to another, or switching status from trial to active.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task SubscriptionUpdatedAsync(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Updated] processing...");
            Stripe.Subscription updated = Mapper<Stripe.Subscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            await _auth.UpdateSubscriptionObjectAsync(updated, stripeEvent.Created);
            _mailObject.AddParagraph("[Subscription Updated] complete!");
        }
        /// <summary>
        /// Occurs whenever a customer ends their subscription.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task SubscriptionDeletedAsync(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription Deleted] processing...");
            Stripe.Subscription deleted = Mapper<Stripe.Subscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            await _auth.RemoveUserSubscriptionObjectAsync(deleted, stripeEvent.Created);
            _mailObject.AddParagraph("[Subscription Deleted] complete!");
        }
        /// <summary>
        /// Occurs three days before the trial period of a subscription is scheduled to end.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public async Task SubscriptionTrialWillEndAsync(Stripe.Event stripeEvent)
        {
            _mailObject.AddParagraph("[Subscription TrialWillEnd] processing...");
            Stripe.Subscription endTrialSubscription = Mapper<Stripe.Subscription>.MapFromJson(stripeEvent.Data.Object.ToString());
            UserSubscription endTrialUserSub = await _auth.GetUserSubscriptionByStripeIdAsync(endTrialSubscription.Id);
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
                message.AddParagraph("If you have not added a card to your account, please log in and do so now in order to continue using the");
                message.AddParagraph("Alternatively, if you no longer wish to continue using the service, please log in and cancel your subscription to ensure you do not get charged when the trial runs out.");
                _email.SendEmailAsync(message).GetAwaiter().GetResult();

                _mailObject.AddParagraph("Sent email to customer: " + endTrialUserSub.User.Email);
            }
            else
            {
                _mailObject.AddParagraph("There is no subscription, so ensure that there is no subscription left on stripe.");
                _billing.Subscriptions.CancelSubscriptionAsync(endTrialSubscription.CustomerId, endTrialSubscription.Id, false).GetAwaiter().GetResult();
                _mailObject.AddParagraph("Send an email to the customer letting them know what has happened...");
                var endTrialUser = await _auth.GetUserByStripeIdAsync(endTrialSubscription.CustomerId);

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
        #endregion
    }
}
