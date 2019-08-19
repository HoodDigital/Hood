using Hood.Core;
using Hood.Extensions;
using Hood.Filters;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    [Authorize]
    [StripeRequired]
    public class BillingController : BaseController
    {
        public BillingController()
            : base()
        { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            BillingHomeModel model = new BillingHomeModel();
            try
            {
                model.Customer = await _account.GetOrCreateStripeCustomerForUser(Engine.Account.Id);
                if (model.Customer != null)
                {

                    model.Invoices = await _stripe.GetAllInvoicesAsync(model.Customer.Id, null);
                    try
                    {
                        model.NextInvoice = await _stripe.GetUpcomingInvoiceAsync(model.Customer.Id);
                    }
                    catch (StripeException)
                    {
                        model.NextInvoice = null;
                    }
                }
                var subs = await _account.GetUserSubscriptionsAsync(new UserSubscriptionListModel() { UserId = Engine.Account.Id, PageSize = int.MaxValue });
                model.Subscriptions = subs.List;
            }
            catch (StripeException stripeEx)
            {
                SaveMessage = $"Stripe error loading billing information for {Engine.Account.UserName}: {stripeEx.Message}";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<BillingController>(SaveMessage, stripeEx);
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error loading billing information for {Engine.Account.UserName}: {ex.Message}";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<BillingController>(SaveMessage, ex);
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> PaymentMethods(string viewName = "_List_PaymentMethods")
        {
            PaymentMethodsModel model = new PaymentMethodsModel();
            try
            {
                model.Customer = await _account.GetOrCreateStripeCustomerForUser(Engine.Account.Id);
                if (model.Customer != null)
                {

                }
                var subs = await _account.GetUserSubscriptionsAsync(new UserSubscriptionListModel() { UserId = Engine.Account.Id, PageSize = int.MaxValue });
                model.PaymentMethods = await _stripe.GetAllPaymentMethodsAsync(model.Customer.Id, "card");
                return View(viewName, model);
            }
            catch (StripeException stripeEx)
            {
                SaveMessage = $"Stripe error loading billing information for {Engine.Account.UserName}: {stripeEx.Message}";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<BillingController>(SaveMessage, stripeEx);
                return BadRequest();
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error loading billing information for {Engine.Account.UserName}: {ex.Message}";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<BillingController>(SaveMessage, ex);
                return BadRequest();
            }
        }

        #region Add Card
        [StripeAccountRequired]
        [Route("account/billing/payment-methods/add-card/")]
        public IActionResult AddCard()
        {
            SetupIntent setupIntent = _stripe.SetupIntentService.Create(new SetupIntentCreateOptions());
            ViewData["ClientSecret"] = setupIntent.ClientSecret;
            return View();
        }
        [HttpPost]
        [StripeAccountRequired]
        [Route("account/billing/payment-methods/add-card/")]
        public async Task<IActionResult> AddCard([FromBody] SetupIntent intent)
        {
            try
            {
                if (!intent.Id.IsSet())
                    throw new Exception("The card could not be added, an invalid payment intent Id was supplied.");

                var confirmOptions = new SetupIntentConfirmOptions { };
                intent = await _stripe.SetupIntentService.GetAsync(intent.Id);
                return await GenerateAddCardResponseAsync(intent);
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
        private async Task<IActionResult> GenerateAddCardResponseAsync(SetupIntent intent)
        {
            if (intent.Status == "requires_action" && intent.NextAction.Type == "use_stripe_sdk")
            {
                return Json(new
                {
                    requires_action = true,
                    payment_intent_client_secret = intent.ClientSecret
                });
            }
            else
            {
                if (intent.Status == "succeeded")
                {
                    // Attach to the customer object now.
                    var customer = await _account.GetOrCreateStripeCustomerForUser(Engine.Account.Id);
                    if (customer == null)
                        throw new Exception("Could not load or create a Stripe customer object for the user.");

                    var options = new PaymentMethodAttachOptions
                    {
                        CustomerId = customer.Id,
                    };
                    await _stripe.PaymentMethodService.AttachAsync(intent.PaymentMethodId, options);

                    return Json(new
                    {
                        success = true,
                        url = Url.Action(nameof(Index))
                    });
                }
                else throw new Exception("The card could not be added, an invalid payment intent status was supplied.");
            }
        }
        #endregion

        #region Update/Delete Payment Methods
        [StripeAccountRequired]
        [Route("account/billing/payment-methods/set-default")]
        public async Task<Response> SetDefault(string id)
        {
            try
            {
                await _stripe.SetDefaultPaymentMethodAsync(Engine.Account.StripeId, id);
                return new Response(true, "This has now been set as your default card.");
            }
            catch (StripeException stripeEx)
            {
                return await ErrorResponseAsync<BillingController>($"Stripe error setting a default card for {Engine.Account.UserName}: {stripeEx.Message}", stripeEx);
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BillingController>($"Error setting a default card for {Engine.Account.UserName}: {ex.Message}", ex);
            }
        }
        [StripeAccountRequired]
        [Route("account/billing/payment-methods/delete")]
        public async Task<Response> DeletePaymentMethod(string id)
        {
            try
            {
                await _stripe.DeletePaymentMethodAsync(Engine.Account.StripeId, id);
                SaveMessage = $"Card deleted.";
                MessageType = Enums.AlertType.Success;
                return new Response(true, "This has now been deleted as a payment method.");
            }
            catch (StripeException stripeEx)
            {
                return await ErrorResponseAsync<BillingController>($"Stripe error deleting a default card for {Engine.Account.UserName}: {stripeEx.Message}", stripeEx);
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BillingController>($"Error deleting a default card for {Engine.Account.UserName}: {ex.Message}", ex);
            }
        }
        #endregion
    }
}
