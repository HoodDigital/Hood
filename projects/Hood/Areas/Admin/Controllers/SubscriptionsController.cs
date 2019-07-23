using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]
    public class SubscriptionsController : BaseController
    {
        public SubscriptionsController()
            : base()
        {
        }

        #region Lists
        [Route("admin/subscriptions/plans/")]
        public async Task<IActionResult> Plans(SubscriptionPlanListModel model)
        {
            return await PlansList(model, "Plans");
        }

        [Route("admin/subscriptions/subscribers/")]
        public async Task<IActionResult> Subscribers(UserSubscriptionListModel model)
        {
            return await SubscribersList(model, "Subscribers");
        }

        [Route("admin/subscriptions/products/")]
        public async Task<IActionResult> Products(SubscriptionProductListModel model)
        {
            return await ProductsList(model, "Products");
        }

        [Route("admin/subscriptions/stripe/plans/")]
        public async Task<IActionResult> Stripe(StripePlanListModel model)
        {
            return await StripeList(model, "Stripe");
        }

        [Route("admin/subscriptions/stripe/products/")]
        public async Task<IActionResult> StripeProducts(StripeProductListModel model)
        {
            return await StripeProductList(model, "StripeProducts");
        }

        [Route("admin/subscriptions/plans/list/")]
        public async Task<IActionResult> PlansList(SubscriptionPlanListModel model, string viewName)
        {
            model = await _account.GetSubscriptionPlansAsync(model);
            return View(viewName.IsSet() ? viewName : "_List_Plans", model);
        }

        [Route("admin/subscriptions/subscribers/list/")]
        public async Task<IActionResult> SubscribersList(UserSubscriptionListModel model, string viewName)
        {
            model = await _account.GetUserSubscriptionsAsync(model);
            return View(viewName.IsSet() ? viewName : "_List_Subscribers", model);
        }

        [Route("admin/subscriptions/products/list/")]
        public async Task<IActionResult> ProductsList(SubscriptionProductListModel model, string viewName)
        {
            model = await _account.GetSubscriptionProductsAsync(model);
            return View(viewName.IsSet() ? viewName : "_List_Products", model);
        }

        [Route("admin/subscriptions/stripe/plans/list/")]
        public async Task<IActionResult> StripeList(StripePlanListModel model, string viewName)
        {
            model = await _account.GetStripeSubscriptionPlansAsync(model);
            return View(viewName.IsSet() ? viewName : "_List_Stripe", model);
        }
        [Route("admin/subscriptions/stripe/products/list/")]
        public async Task<IActionResult> StripeProductList(StripeProductListModel model, string viewName)
        {
            model = await _account.GetStripeProductsAsync(model);
            return View(viewName.IsSet() ? viewName : "_List_Stripe", model);
        }
        #endregion

        #region Edit Subscription (Plan)
        [Route("admin/subscriptions/plans/edit/{id}/")]
        public async Task<IActionResult> Edit(int id)
        {
            Subscription model = await _account.GetSubscriptionPlanByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            model.StripePlan = await _stripe.GetPlanByIdAsync(model.StripeId);

            return View(model);
        }

        [HttpPost()]
        [Route("admin/subscriptions/plans/edit/{id}/")]
        public async Task<ActionResult> Edit(Subscription model)
        {
            try
            {
                model.LastEditedBy = Engine.Account.UserName;
                model.LastEditedOn = DateTime.Now;

                await _account.UpdateSubscriptionPlanAsync(model);
                SaveMessage = "Subscription saved.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "Errorsaving: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(
                   string.Format("Error saving subscription ({0}) with name: {1}", model.StripeId.IsSet() ? model.StripeId : "No Stripe Id", model.Name),
                   ex,
                   LogType.Error,
                   _userManager.GetUserId(User),
                   Url.AbsoluteAction("Index", "Subscriptions")
               );
            }

            model.StripePlan = await _stripe.GetPlanByIdAsync(model.StripeId);

            return View(model);
        }
        #endregion

        #region Create Subscription (Plan)
        [Route("admin/subscriptions/plans/create")]
        public IActionResult Create()
        {
            return View("_Blade_Plan", new Subscription() { IntervalCount = 1, Public = true });
        }

        [HttpPost]
        [Route("admin/subscriptions/plans/create")]
        public async Task<Response> Create(Subscription model)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);
                BillingSettings billingSettings = Engine.Settings.Billing;

                model.CreatedBy = user.Id;
                model.Created = DateTime.Now;
                model.LastEditedBy = user.UserName;
                model.LastEditedOn = DateTime.Now;
                model.Amount = (int)Math.Floor(model.CreatePrice * 100);
                model.LiveMode = billingSettings.EnableStripeTestMode;

                model = await _account.CreateSubscriptionPlanAsync(model);
                await _logService.AddLogAsync<SubscriptionsController>(
                    $"Subscription ({model.StripeId}) added with name: {model.Name}",
                    model
                );
                return new Response(true, $"The subscription plan was created successfully.<br /><a href='{Url.Action(nameof(Edit), new { id = model.Id })}'>Go to the new plan</a>");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<SubscriptionsController>(
                    $"Error adding subscription ({model.StripeId}) with name: {model.Name}",
                    ex
                );
            }
        }
        #endregion

        #region Delete Subscription (Plan)
        [Route("admin/subscriptions/plans/delete")]
        [HttpPost()]
        public async Task<Response> Delete(int id)
        {
            try
            {
                Subscription subscription = await _account.GetSubscriptionPlanByIdAsync(id);
                if (subscription == null)
                {
                    throw new Exception("Could not find the subscription plan.");
                }

                await _account.DeleteSubscriptionPlanAsync(id);
                SaveMessage = string.Format("Subscription ({0}) deleted with name: {1}", subscription.StripeId, subscription.Name);
                await _logService.AddLogAsync<SubscriptionsController>(
                    SaveMessage,
                    JsonConvert.SerializeObject(subscription),
                    LogType.Success,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("Edit", "Subscriptions", new { id = subscription.Id })
                );
                return new Response(true, $"The subscription plan has been deleted.");
            }
            catch (Exception ex)
            {
                SaveMessage = string.Format("Error deleting subscription plan with id: {0}", id);
                await _logService.AddExceptionAsync<SubscriptionsController>(
                    SaveMessage,
                    ex,
                    LogType.Error,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("Index", "Subscriptions")
                );
                return new Response(ex);
            }
        }
        #endregion

        #region Edit Subscription (Product)
        [Route("admin/subscriptions/products/edit/{id}/")]
        public async Task<IActionResult> EditProduct(int id)
        {
            SubscriptionProduct model = await _account.GetSubscriptionProductByIdAsync(id);
            model.StripeProduct = await _stripe.GetProductByIdAsync(model.StripeId);
            return View(model);
        }

        [HttpPost()]
        [Route("admin/subscriptions/products/edit/{id}/")]
        public async Task<ActionResult> EditProduct(SubscriptionProduct model)
        {
            try
            {
                model.LastEditedBy = Engine.Account.UserName;
                model.LastEditedOn = DateTime.Now;

                await _account.UpdateSubscriptionProductAsync(model);
                SaveMessage = "Subscription product group saved.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "Error saving: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(
                   $"Error saving subscription product ({model.StripeId}) with name: {model.DisplayName}",
                   ex
                );
            }
            model.StripeProduct = await _stripe.GetProductByIdAsync(model.StripeId);
            return View(model);
        }
        #endregion

        #region Create Subscription (Product)
        [Route("admin/subscriptions/products/create/")]
        public IActionResult CreateProduct()
        {
            return View("_Blade_Product", new SubscriptionProduct());
        }

        [HttpPost]
        [Route("admin/subscriptions/products/create/")]
        public async Task<Response> CreateProduct(SubscriptionProduct model)
        {
            try
            {
                await _account.CreateSubscriptionProductAsync(model.DisplayName, model.StripeId);
                await _logService.AddLogAsync<SubscriptionsController>($"Subscription product ({model.StripeId}) added with name: {model.DisplayName}", model);
                return new Response(true, $"The subscription product was created successfully.<br /><a href='{Url.Action(nameof(EditProduct), new { id = model.Id })}'>Go to the new product</a>");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<SubscriptionsController>($"Error adding subscription product ({model.StripeId}) with name: {model.DisplayName}", ex);
            }
        }
        #endregion

        #region Delete Subscription (Product)
        [Route("admin/subscriptions/products/delete")]
        [HttpPost()]
        public async Task<Response> DeleteProduct(int id)
        {
            try
            {
                SubscriptionProduct subscriptionProduct = await _account.GetSubscriptionProductByIdAsync(id);
                if (subscriptionProduct == null)
                {
                    throw new Exception("Could not find the product group.");
                }

                await _account.DeleteSubscriptionProductAsync(id);

                SaveMessage = string.Format("Subscription ({0}) deleted with name: {1}", subscriptionProduct.StripeId, subscriptionProduct.DisplayName);
                await _logService.AddLogAsync<SubscriptionsController>(
                    SaveMessage,
                    subscriptionProduct,
                    LogType.Success,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("DeleteProduct", "Subscriptions", new { id })
                );
                return new Response(true, $"The subscription has been deleted.");
            }
            catch (Exception ex)
            {
                SaveMessage = string.Format("Error deleting subscription with id: {0}", id);
                await _logService.AddExceptionAsync<SubscriptionsController>(
                    SaveMessage,
                    ex,
                    LogType.Error,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("Index", "Subscriptions")
                );
                return new Response(ex);
            }
        }
        #endregion

        #region Edit Subscription (Plan)
        [Route("admin/subscriptions/subscribers/edit/{id}/")]
        public async Task<IActionResult> EditSubscription(int id)
        {
            UserSubscription model = await _account.GetUserSubscriptionByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost()]
        [Route("admin/subscriptions/subscribers/edit/{id}/")]
        public async Task<ActionResult> EditSubscription(UserSubscription model)
        {
            try
            {
                await _account.UpdateUserSubscriptionAsync(model);
                SaveMessage = "Subscription saved.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "Errorsaving: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(
                   string.Format("Error saving user subscription with stripe Id: {0}", model.StripeId.IsSet() ? model.StripeId : "No Stripe Id"),
                   ex,
                   LogType.Error,
                   _userManager.GetUserId(User),
                   Url.AbsoluteAction("Index", "Subscriptions")
               );
            }
            return View(model);
        }
        #endregion

        #region Syncing
        [Route("admin/subscriptions/products/sync")]
        public async Task<IActionResult> SyncProduct(int? id, string stripeId)
        {
            try
            {
                SubscriptionProduct product = await _account.SyncSubscriptionProductAsync(id, stripeId);

                SaveMessage = "Product successfully synced with Stripe.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "There was an error while syncing: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(
                    $"Error syncing subscription product with Id: {id} / {stripeId}",
                    ex,
                    LogType.Error,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("SyncProduct", "Subscriptions")
                );
            }
            return RedirectToAction(nameof(Products));
        }
        [Route("admin/subscriptions/plans/sync")]
        public async Task<IActionResult> SyncPlan(int? id, string stripeId)
        {
            try
            {
                Subscription subscription = await _account.SyncSubscriptionPlanAsync(id, stripeId);

                SaveMessage = "Product successfully synced with Stripe.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "There was an error while syncing: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(
                   $"Error syncing subscription plan with Id: {id} / {stripeId}",
                   ex,
                   LogType.Error,
                   _userManager.GetUserId(User),
                   Url.AbsoluteAction("SyncPlan", "Subscriptions")
               );
            }
            return RedirectToAction(nameof(Plans));
        }
        [Route("admin/subscriptions/subscribers/sync")]
        public async Task<IActionResult> SyncSubscription(int id)
        {
            try
            {
                UserSubscription subscription = await _account.SyncUserSubscriptionAsync(id);
                SaveMessage = "Subscription successfully synced with Stripe.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "There was an error while syncing: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(
                  $"Error syncing user subscription with Id: {id}",
                  ex,
                  LogType.Error,
                  _userManager.GetUserId(User),
                  Url.AbsoluteAction("SyncSubscription", "Subscriptions")
              );
            }
            return RedirectToAction(nameof(Subscribers));
        }
        #endregion
    }
}


