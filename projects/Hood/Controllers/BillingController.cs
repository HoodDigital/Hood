using Hood.Core;
using Hood.Enums;
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
                model.Customer = await _account.GetCustomerObjectAsync(Engine.Account.StripeId);
                if (model.Customer != null)
                {
                    model.Invoices = await _billing.Invoices.GetAllAsync(model.Customer.Id, null);
                    try
                    {
                        model.NextInvoice = await _billing.Invoices.GetUpcoming(model.Customer.Id);
                    }
                    catch (StripeException)
                    {
                        model.NextInvoice = null;
                    }
                }
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error loading billing information for {Engine.Account.UserName}.";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<BillingController>(SaveMessage, ex);
            }
            return View(model);
        }
        
        internal async Task<IActionResult> NoCustomerAccount()
        {
            SaveMessage = $"Your customer account object was not found, so we have created one for you.";
            MessageType = Enums.AlertType.Info;
            return await Index();
        }

        public async Task<IActionResult> AddCard()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCard(SubscriptionModel model, string returnUrl = null)
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);

                // Check if the user has a stripeId - if they do, we dont need to create them again, we can simply add a new card token to their account, or use an existing one maybe.
                if (user.StripeId.IsSet())
                    model.Customer = await _billing.Customers.FindByIdAsync(user.StripeId);

                // if not, then the user must have supplied a token
                Stripe.Token stripeToken = _billing.Stripe.TokenService.Get(model.StripeToken);
                if (stripeToken == null)
                    throw new Exception("The provided card token was not valid.");

                // Check if the customer object exists.
                if (model.Customer == null)
                {
                    // if it does not, create it, add the card and subscribe the plan.
                    model.Customer = await _billing.Customers.CreateCustomer(user, model.StripeToken);
                    user.StripeId = model.Customer.Id;
                    IdentityResult result = await _userManager.UpdateAsync(user);
                }
                else
                {
                    // finally, add the user to the subscription, using the new card as the charge source.
                    await _billing.Cards.CreateCard(model.Customer.Id, model.StripeToken);
                }

                SaveMessage = $"The card has been added successfully.";
                MessageType = Enums.AlertType.Success;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error loading billing information for {Engine.Account.UserName}.";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<BillingController>(SaveMessage, ex);
            }
            return View(model);
        }

        [StripeAccountRequired]
        public async Task<IActionResult> SetDefaultCard(string uid)
        {
            try
            {
                await _billing.Customers.SetDefaultCard(Engine.Account.StripeId, uid);
                SaveMessage = $"Default card set.";
                MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error setting a default card.";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<BillingController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }

        [StripeAccountRequired]
        public async Task<IActionResult> DeleteCard(string uid)
        {
            try
            {
                await _billing.Cards.DeleteCard(Engine.Account.StripeId, uid);
                SaveMessage = $"Card deleted.";
                MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error deleting a card.";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<BillingController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
