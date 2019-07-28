using Hood.Core;
using Hood.Events;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Stripe;
using System;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class StripeWebHookService : IStripeWebHookService
    {
        protected readonly IAccountRepository _account;

        protected readonly BasicSettings _basicSettings;
        protected readonly BillingSettings _billingSettings;
        protected readonly MailSettings _mailSettings;

        protected readonly HttpContext _context;
        protected readonly IEventsService _eventService;
        protected readonly IEmailSender _emailSender;
        protected readonly IMailService _mailService;
        protected readonly IStripeService _stripe;
        protected readonly ILogService _logService;
        protected readonly UserManager<ApplicationUser> _userManager;

        public StripeWebHookService(IHttpContextAccessor contextAccessor)
        {
            _context = contextAccessor.HttpContext;

            _account = Engine.Services.Resolve<IAccountRepository>();
            _eventService = Engine.Services.Resolve<IEventsService>();
            _emailSender = Engine.Services.Resolve<IEmailSender>();
            _mailService = Engine.Services.Resolve<IMailService>();
            _stripe = Engine.Services.Resolve<IStripeService>();
            _logService = Engine.Services.Resolve<ILogService>();
            _userManager = Engine.Services.Resolve<UserManager<ApplicationUser>>();

            _basicSettings = Engine.Settings.Basic;
            _mailSettings = Engine.Settings.Mail;
            _billingSettings = Engine.Settings.Billing;
        }

        public async Task ProcessEventAsync(Event stripeEvent)
        {
            try
            {
                StripeWebHookTriggerArgs args = new StripeWebHookTriggerArgs(stripeEvent);

                if (stripeEvent.GetEventName() == "invalid.event.object")
                {
                    throw new Exception("The event object was invalid.");
                }

                await ProcessEventByTypeAsync(stripeEvent);

                // Fire the event to allow any other packages to process the webhook.
                _eventService.TriggerStripeWebhook(this, args);

                await _logService.AddLogAsync<StripeWebHookService>($"Stripe webhook {stripeEvent.GetEventName()} successfully processed.", stripeEvent, LogType.Success);
            }
            catch (AlertedException ex)
            {
                // alert the media.
                BasicSettings info = Engine.Settings.Basic;
                MailObject mailObject = new MailObject()
                {
                    PreHeader = "Stripe Webhook Error"
                };
                mailObject.Subject = mailObject.PreHeader;
                mailObject.AddH1("A Stripe Webhook ran into a serious error!");
                mailObject.AddParagraph($"Webhook received on site: <strong>{stripeEvent.GetEventName()}</strong>");
                mailObject.AddParagraph($"Message: <strong>{ex.Message}</strong>");
                mailObject.AddParagraph($"Url: <strong>{_context.GetSiteUrl()}</strong>");
                mailObject.AddParagraph($"Exception: <strong>{_context.GetSiteUrl()}</strong>");
                mailObject.AddDiv(stripeEvent.ToJson().ToHtml());
                mailObject.Template = MailSettings.DangerTemplate;
                await _emailSender.NotifyRoleAsync(mailObject, "Admin");

                await _logService.AddExceptionAsync<StripeWebHookService>($"An error occurred processing a stripe webhook: {stripeEvent.GetEventName()}", ex);
                throw ex;
            }
            catch (LoggedException ex)
            {
                await _logService.AddExceptionAsync<StripeWebHookService>($"An minor error occurred processing a stripe webhook: {stripeEvent.GetEventName()}", ex, LogType.Warning);
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<StripeWebHookService>($"An error occurred processing a stripe webhook: {stripeEvent.GetEventName()}", ex);
                throw ex;
            }
        }

        protected async Task ProcessEventByTypeAsync(Stripe.Event stripeEvent)
        {
            switch (stripeEvent.GetEventName())
            {
                case "customer.created":
                case "customer.updated":
                    await CustomerCreatedOrUpdatedAsync(stripeEvent.Data.Object as Customer);
                    break;
                case "customer.deleted":
                    await CustomerDeletedAsync(stripeEvent.Data.Object as Customer);
                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                case "customer.subscription.deleted":
                    await SubscriptionUpdatedAsync(stripeEvent.Data.Object as Stripe.Subscription);
                    break;
                case "customer.subscription.trial_will_end":
                    await SubscriptionTrialWillEndAsync(stripeEvent.Data.Object as Stripe.Subscription);
                    break;

                case "invoice.payment_failed":
                    await InvoicePaymentFailedAsync(stripeEvent.Data.Object as Invoice);
                    break;
                case "invoice.payment_succeeded":
                    await InvoicePaymentSucceededAsync(stripeEvent.Data.Object as Invoice);
                    break;

                case "plan.created":
                case "plan.updated":
                    await PlanCreatedOrUpdatedAsync(stripeEvent.Data.Object as Plan);
                    break;
                case "plan.deleted":
                    await PlanDeletedAsync(stripeEvent.Data.Object as Plan);
                    break;

                case "product.created":
                case "product.updated":
                    await ProductCreatedOrUpdatedAsync(stripeEvent.Data.Object as Product);
                    break;
                case "product.deleted":
                    await ProductDeletedAsync(stripeEvent.Data.Object as Product);
                    break;

                default:
                    await _logService.AddLogAsync<StripeWebHookService>($"The event name could not resolve to a web hook handler: {stripeEvent.GetEventName()}", type: LogType.Warning);
                    break;
            }
        }

        #region Customer Webhooks
        /// <summary>
        /// Occurs whenever a new customer is created or updated. Will either create or update a local User account for the customer object.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task CustomerCreatedOrUpdatedAsync(Customer customer)
        {
            // Find a user by the stripe ID.
            ApplicationUser user = await _account.GetUserByStripeIdAsync(customer.Id);
            if (user != null)
            {
                user.StripeId = customer.Id;
                user.PhoneNumber = customer.Phone;
                await _account.UpdateUserAsync(user);
                await _logService.AddLogAsync<StripeWebHookService>($"Stripe customer {customer.Id} ({customer.Email}) updated local account {user.UserName} via a WebHook.");
                return;
            }

            // If not, check for an account with an email.
            user = await _account.GetUserByEmailAsync(customer.Email);

            if (user != null)
            {
                // if exists, check that doesnt have a stripe id
                if (user.StripeId.IsSet())
                {
                    // If it does, check it is not pointing at a stripe account. 
                    Customer connectedCustomer = await _stripe.GetCustomerByIdAsync(user.StripeId);
                    if (connectedCustomer != null)
                    {
                        // If so warn admins.
                        throw new AlertedException($"The new customer {customer.Id} ({customer.Email}) cannot be linked, the local account is connected to another Stripe customer account, disconnect it to retry this.");
                    }
                    user.StripeId = customer.Id;
                    await _account.UpdateUserAsync(user);
                    await _logService.AddLogAsync<StripeWebHookService>($"Stripe customer {customer.Id} ({customer.Email}) linked to local account {user.UserName}");
                }
            }
            else
            {
                user = await _account.CreateLocalUserForCustomerObject(customer);
                if (user == null)
                {
                    throw new AlertedException($"Stripe customer {customer.Id} ({customer.Email}) could not be linked to a new local account, the create user function failed.");
                }
                await _logService.AddLogAsync<StripeWebHookService>($"Local user {user.UserName} was created from Stripe Webhook: customer.created");
            }
        }

        /// <summary>
        /// Occurs whenever a customer is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task CustomerDeletedAsync(Customer deletedCustomer)
        {
            ApplicationUser user = await _account.GetUserByStripeIdAsync(deletedCustomer.Id);
            if (user != null)
            {
                user = await _account.GetUserByEmailAsync(deletedCustomer.Email);
            }

            if (user != null)
            {
                user.StripeId = null;
                await _account.UpdateUserAsync(user);
                await _logService.AddLogAsync<StripeWebHookService>($"Stripe customer {deletedCustomer.Id} ({deletedCustomer.Email}) was deleted from Stripe, and has been disconnected from local account {user.UserName} via a WebHook.");
            }
            else
            {
                throw new LoggedException($"Could not load customer {deletedCustomer.Id} while processing a Stripe Webhook, perhaps the customer has already been deleted from the system: customer.deleted");
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
        protected async Task InvoicePaymentFailedAsync(Invoice failedInvoice)
        {
            if (failedInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                Stripe.Subscription failedInvoiceSubscription = await _stripe.GetSusbcriptionByIdAsync(failedInvoice.SubscriptionId);

                UserSubscription failedInvoiceUserSub = await _account.GetUserSubscriptionByStripeIdAsync(failedInvoiceSubscription.Id);
                if (failedInvoiceUserSub != null)
                {
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
                    await _emailSender.SendEmailAsync(message);

                }
                else
                {
                    await _stripe.CancelSubscriptionAsync(failedInvoice.SubscriptionId, false);
                    ApplicationUser failedInvoiceUser = await _account.GetUserByStripeIdAsync(failedInvoice.CustomerId);

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
                    await _emailSender.SendEmailAsync(message);
                }
            }

        }
        /// <summary>
        /// Occurs whenever an invoice attempts to be paid, and the payment succeeds.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task InvoicePaymentSucceededAsync(Invoice successfulInvoice)
        {
            if (successfulInvoice.SubscriptionId.IsSet())
            {
                // Get the subscription.
                Stripe.Subscription subscription = await _stripe.GetSusbcriptionByIdAsync(successfulInvoice.SubscriptionId);

                UserSubscription userSub = await _account.GetUserSubscriptionByStripeIdAsync(subscription.Id);
                // if ths sub is set up in the db ALL IS WELL. Continuer
                if (userSub == null)
                {

                    // if ths sub is NOT set up in the db warn the administrators that a charge has been issued without a matching subscription.

                    //Stripe.Refund refund = _stripe.Stripe.RefundService.CreateAsync(new RefundCreateOptions { ChargeId = successfulInvoice.ChargeId }).Result;
                    //await _stripe.CancelSubscriptionAsync(successfulInvoice.SubscriptionId, false);

                    throw new AlertedException($"A charge was created, but it did not match a subscription. The charge can be found on Stripe here: {successfulInvoice.HostedInvoiceUrl}");

                }
                else
                {
                    ApplicationUser successfulInvoiceUser = await _account.GetUserByStripeIdAsync(successfulInvoice.CustomerId);
                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(successfulInvoiceUser.Email),
                        PreHeader = "Thank you for your payment on " + Engine.Settings.Basic.FullTitle,
                        Subject = "Thank you for your payment..."
                    };
                    message.AddH1("Thank you!");
                    message.AddParagraph("Your payment has been received. You will recieve a payment reciept from our payment providers, Stripe.");
                    message.Template = MailSettings.DangerTemplate;
                    await _emailSender.SendEmailAsync(message);
                }
            }
        }
        #endregion

        #region Stripe.Plan Webhooks
        /// <summary>
        /// Occurs whenever a plan is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task PlanCreatedOrUpdatedAsync(Plan plan)
        {
            await _account.SyncSubscriptionProductAsync(null, plan.Id);
        }
        /// <summary>
        /// Occurs whenever a plan is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task PlanDeletedAsync(Plan deletedPlan)
        {
            Models.Subscription plan = await _account.SyncSubscriptionPlanAsync(null, deletedPlan.Id);
            if (plan != null)
            {
                plan.StripeId = null;
                await _account.UpdateSubscriptionPlanAsync(plan);
                await _logService.AddLogAsync<StripeWebHookService>($"Stripe plan {deletedPlan.Id} ({deletedPlan.Nickname}) was deleted from Stripe, and has been disconnected from local plan {plan.Id} ({plan.Name}) via a WebHook.");
            }
            else
            {
                throw new LoggedException($"Could not load plan {deletedPlan.Id} while processing a Stripe Webhook: plan.deleted");
            }
        }
        #endregion

        #region Stripe.Product Webhooks
        /// <summary>
        /// Occurs whenever a plan is created.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task ProductCreatedOrUpdatedAsync(Product newProduct)
        {
            await _account.SyncSubscriptionProductAsync(null, newProduct.Id);
        }
        /// <summary>
        /// Occurs whenever a plan is deleted.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task ProductDeletedAsync(Product deletedProduct)
        {
            SubscriptionProduct product = await _account.SyncSubscriptionProductAsync(null, deletedProduct.Id);
            if (product != null)
            {
                product.StripeId = null;
                await _account.UpdateSubscriptionProductAsync(product);
                await _logService.AddLogAsync<StripeWebHookService>($"Stripe product {deletedProduct.Id} ({deletedProduct.Name}) was deleted from Stripe, and has been disconnected from local product {product.Id} ({product.DisplayName}) via a WebHook.");
            }
            else
            {
                throw new LoggedException($"Could not load product {deletedProduct.Id} while processing a Stripe Webhook: product.deleted");
            }
        }
        #endregion

        #region Stripe.Subscription Webhooks
        /// <summary>
        /// Occurs whenever a customer with no subscription is signed up for a plan.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task SubscriptionUpdatedAsync(Stripe.Subscription subscription)
        {
            await _account.SyncUserSubscriptionAsync(null, subscription.Id);
        }
        /// <summary>
        /// Occurs three days before the trial period of a subscription is scheduled to end.
        /// </summary>
        /// <param name="stripeEvent"></param>
        protected async Task SubscriptionTrialWillEndAsync(Stripe.Subscription endTrialSubscription)
        {
            UserSubscription endTrialUserSub = await _account.SyncUserSubscriptionAsync(null, endTrialSubscription.Id);
            if (endTrialUserSub != null)
            {
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
                await _emailSender.SendEmailAsync(message);
            }
        }
        #endregion
    }
}
