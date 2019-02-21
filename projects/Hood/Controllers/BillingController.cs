using Hood.Enums;
using Hood.Extensions;
using Hood.Filters;
using Hood.Models;
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
    public class BillingController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public BillingController()
            : base()
        { }

        [HttpGet]
        public async Task<IActionResult> Index(BillingMessage? message = null)
        {
            AccountInfo account = HttpContext.GetAccountInfo();
            BillingHomeModel model = new BillingHomeModel()
            {
                User = account.User
            };
            try
            {
                model.Customer = await _account.LoadCustomerObject(model.User?.StripeId, true);
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
                model.Message = message;
                return View(model);
            }
            catch (Exception ex)
            {
                model.Message = ex.Message.ToEnum(BillingMessage.Error);
                return View(model);
            }
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
                StripeToken stripeToken = _billing.Stripe.TokenService.Get(model.StripeToken);
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

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                BillingMessage bm = BillingMessage.Null;
                if (Enum.TryParse(ex.Message, out bm))
                {
                    model.AddBillingMessage(bm);
                }
                else
                {
                    model.AddBillingMessage(BillingMessage.StripeError);
                }
                return View(model);
            }
        }

        [StripeAccountRequired]
        public async Task<IActionResult> SetDefaultCard(string uid)
        {
            try
            {
                AccountInfo account = HttpContext.GetAccountInfo();
                await _billing.Customers.SetDefaultCard(account.User.StripeId, uid);
                return RedirectToAction("Index", "Billing");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Billing", new { Message = BillingMessage.Error });
            }
        }

        [StripeAccountRequired]
        public async Task<IActionResult> DeleteCard(string uid)
        {
            try
            {
                AccountInfo account = HttpContext.GetAccountInfo();
                await _billing.Cards.DeleteCard(account.User.StripeId, uid);
                return RedirectToAction("Index", "Billing");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Billing", new { Message = BillingMessage.Error });
            }
        }

    }
}
