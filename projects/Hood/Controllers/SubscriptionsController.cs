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
using System.IO;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    [StripeRequired]
    public class SubscriptionsController : BaseController
    {

        private readonly IStripeWebHookService _webHooks;
        public SubscriptionsController(IStripeWebHookService webHooks)
            : base()
        {
            _webHooks = webHooks;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            SubscriptionProductsModel model = new SubscriptionProductsModel()
            {
                Products = (await _account.GetSubscriptionProductsAsync(new SubscriptionProductListModel() { PageSize = int.MaxValue })).List
            };
            return View(model);
        }

        [Route("account/subscriptions/products/")]
        public async Task<IActionResult> Select(string returnUrl = null)
        {
            SubscriptionProductsModel model = new SubscriptionProductsModel()
            {
                Products = (await _account.GetSubscriptionProductsAsync(new SubscriptionProductListModel() { PageSize = int.MaxValue })).List,
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [Route("account/subscriptions/show/{id}/{title}")]
        public async Task<IActionResult> Product(int id, string title, string returnUrl = null)
        {
            SubscriptionPlansModel model = new SubscriptionPlansModel()
            {
                Product = await _account.GetSubscriptionProductByIdAsync(id),
                Plans = (await _account.GetSubscriptionPlansAsync(new SubscriptionPlanListModel() { PageSize = int.MaxValue, ProductId = id })).List,
                Customer = await _account.GetOrCreateStripeCustomerForUser(Engine.Account.Id),
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        #region Buy Subscription / Subsribe
        [Authorize]
        [Route("account/subscriptions/buy/{id}/{title}")]
        public async Task<IActionResult> Buy(int id, string title)
        {
            BuySubscriptionModel model = new BuySubscriptionModel()
            {
                Plan = await _account.GetSubscriptionPlanByIdAsync(id),
                Customer = await _account.GetOrCreateStripeCustomerForUser(Engine.Account.Id)
            };
            if (User.IsSubscribedToProduct(model.Plan.SubscriptionProductId))
                throw new Exception("You already have an active subscription to this product. You can change to another package from your Manage Subscriptions page");
            return View(model);
        }
        [HttpPost]
        [StripeAccountRequired]
        [Route("account/subscriptions/buy/{id}/{title}")]
        public async Task<IActionResult> Buy([FromBody] ConfirmSubscriptionModel model)
        {
            try
            {
                Models.Subscription plan = await _account.GetSubscriptionPlanByIdAsync(model.PlanId);
                if (User.IsSubscribedToProduct(plan.SubscriptionProductId))
                    throw new Exception("You already have an active subscription to this product. You can change to another package from your Manage Subscriptions page");
                Customer customer = await _account.GetOrCreateStripeCustomerForUser(Engine.Account.Id);
                if (customer == null)
                {
                    throw new Exception("There was a problem loading the customer object.");
                }
                Subscription subscription = null;
                if (model.Token.IsSet())
                {
                    // Add the new token source to the customer, then create the plan using the default customer source.
                    CustomerUpdateOptions options = new CustomerUpdateOptions
                    {
                        Source = model.Token,
                    };
                    customer = await _stripe.CustomerService.UpdateAsync(customer.Id, options);
                    subscription = await _stripe.AddCustomerToPlan(customer.Id, plan.StripeId);
                }
                else
                {
                    // Pass the payment method id, if there is one, otherwise it will just go with whatever is on file.
                    subscription = await _stripe.AddCustomerToPlan(customer.Id, plan.StripeId, paymentMethodId: model.PaymentMethodId);
                }

                if (subscription.Status == Stripe.SubscriptionStatuses.Trialing || subscription.LatestInvoice.PaymentIntent.Status == "succeeded")
                {
                    var userSub = await _account.CreateUserSubscription(plan.Id, Engine.Account.Id, subscription);
                    return Json(new
                    {
                        success = true,
                        url = Url.Action(nameof(Welcome), new { id = userSub.Id })
                    });
                }
                else if (subscription.LatestInvoice.PaymentIntent.Status == "requires_payment_method")
                {
                    return Json(new
                    {
                        requires_payment_method = true,
                        payment_intent_client_secret = subscription.LatestInvoice.PaymentIntent.ClientSecret
                    });
                }
                else if (subscription.LatestInvoice.PaymentIntent.Status == "requires_action" && subscription.LatestInvoice.PaymentIntent.NextAction.Type == "use_stripe_sdk")
                {
                    return Json(new
                    {
                        requires_action = true,
                        payment_intent_client_secret = subscription.LatestInvoice.PaymentIntent.ClientSecret
                    });
                }
                else
                    throw new Exception("An invalid response was received when setting up the subscription.");
            }
            catch (StripeException e)
            {
                return Json(new { error = e.StripeError.Message });
            }
            catch (Exception e)
            {
                return Json(new { error = e.Message });
            }
        }

        [Authorize]
        [StripeAccountRequired]
        public async Task<IActionResult> Welcome(int id)
        {
            try
            {
                Models.UserSubscription sub = await _account.GetUserSubscriptionByIdAsync(id);
                if (sub == null)
                {
                    throw new Exception("Could not find the subscription.");
                }
                SubscriptionWelcomeModel model = new SubscriptionWelcomeModel()
                {
                    CurrentUserSubscription = sub,
                    CurrentSubscription = await _stripe.GetSusbcriptionByIdAsync(sub.StripeId),
                    Customer = await _account.GetOrCreateStripeCustomerForUser(Engine.Account.Id)
                };
                return View(model);
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error loading subscription: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Update/Change Subscription
        [Authorize]
        [StripeAccountRequired]
        public async Task<IActionResult> Change(int id, int planId, string returnUrl = null)
        {
            try
            {
                Models.UserSubscription sub = await _account.GetUserSubscriptionByIdAsync(id);
                if (sub == null)
                {
                    throw new Exception("Could not find the subscription.");
                }

                if (sub.UserId != User.GetUserId())
                {
                    throw new Exception("You are not the user associated with this subscription.");
                }

                ChangeSubscriptionModel model = new ChangeSubscriptionModel()
                {
                    CurrentSubscription = sub,
                    Plan = await _account.GetSubscriptionPlanByIdAsync(planId),
                    Customer = await _account.GetOrCreateStripeCustomerForUser(Engine.Account.Id),
                    ReturnUrl = returnUrl,
                    PlanId = planId,
                    SubscriptionId = id
                };
                return View(model);
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error loading subscription: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize]
        [StripeAccountRequired]
        public async Task<IActionResult> Change(ChangeSubscriptionModel model)
        {
            try
            {
                Models.UserSubscription sub = await _account.GetUserSubscriptionByIdAsync(model.SubscriptionId);
                if (sub == null)
                {
                    throw new Exception("Could not find the subscription.");
                }

                if (sub.UserId != User.GetUserId())
                {
                    throw new Exception("You are not the user associated with this subscription.");
                }

                Models.Subscription plan = await _account.GetSubscriptionPlanByIdAsync(model.PlanId);
                if (sub == null)
                {
                    throw new Exception("Could not find the subscription.");
                }

                await _stripe.SwitchSubscriptionPlanAsync(sub.StripeId, plan.StripeId);
                await _account.SyncUserSubscriptionAsync(model.SubscriptionId, sub.StripeId);

                SaveMessage = $"Successfully switched onto the new subscription.";
                MessageType = AlertType.Success;

                if (model.ReturnUrl.IsSet())
                {
                    return Redirect(model.ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error upgrading subscription: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);
            }

            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        [StripeAccountRequired]
        public async Task<IActionResult> Cancel(int id, string returnUrl = null)
        {
            try
            {
                Models.UserSubscription sub = await _account.CancelUserSubscriptionAsync(id);

                SaveMessage = $"Subscription cancelled by user {User.Identity.Name}";
                MessageType = AlertType.Success;
                _eventService.TriggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs(SaveMessage, sub));

                if (returnUrl.IsSet())
                {
                    return Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error cancelling a subscription.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        [StripeAccountRequired]
        public async Task<IActionResult> Reactivate(int id, string returnUrl = null)
        {
            try
            {
                Models.UserSubscription sub = await _account.ReactivateUserSubscriptionAsync(id);

                SaveMessage = $"Subscription re-activated by user {User.Identity.Name}";
                MessageType = AlertType.Success;
                _eventService.TriggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs(SaveMessage, sub));

                if (returnUrl.IsSet())
                {
                    return Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error reactivating a subscription.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        [StripeAccountRequired]
        public async Task<IActionResult> Remove(int id, string returnUrl = null)
        {
            try
            {
                Models.UserSubscription sub = await _account.CancelUserSubscriptionAsync(id, false);

                SaveMessage = $"Subscription removed by user {User.Identity.Name}";
                MessageType = AlertType.Success;
                _eventService.TriggerUserSubcriptionChanged(this, new UserSubscriptionChangeEventArgs(SaveMessage, sub));

                if (returnUrl.IsSet())
                {
                    return Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error cancelling a subscription.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion

        [HttpPost]
        [AllowAnonymous]
        [Route("stripe/webhooks/")]
        public async Task<StatusCodeResult> WebHooks()
        {
            string json = new StreamReader(Request.Body).ReadToEnd();

            try
            {
                if (!Engine.Settings.Billing.StripeWebhookSecret.IsSet())
                {
                    throw new Exception("Cannot process a signed webhook without a Stripe Webhook Secret. Set one in the Billing Settings to fix this.");
                }

                Event stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], Engine.Settings.Billing.StripeWebhookSecret);
                await _webHooks.ProcessEventAsync(stripeEvent);
                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

    }

}


