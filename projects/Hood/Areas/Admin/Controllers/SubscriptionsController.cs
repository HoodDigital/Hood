using Hood.Core;
using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class SubscriptionsController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public SubscriptionsController()
            : base()
        {
        }

        [Route("admin/subscriptions/")]
        public async Task<IActionResult> Index(SubscriptionSearchModel model)
        {
            model = await _account.GetPagedSubscriptionPlans(model);
            return View(model);
        }

        [Route("admin/subscribers/")]
        public async Task<IActionResult> Subscribers(SubscriberSearchModel model)
        {
            model = await _account.GetPagedSubscribers(model);
            return View(model);
        }

        [Route("admin/subscriptions/edit/{id}/")]
        public async Task<IActionResult> Edit(int id, EditorMessage? message)
        {
            var model = await _account.GetSubscriptionPlanById(id);
            model.AddEditorMessage(message);
            return View(model);
        }

        [HttpPost()]
        [Route("admin/subscriptions/edit/{id}/")]
        public async Task<ActionResult> Edit(Subscription model)
        {
            try
            {
                await _account.UpdateSubscription(model);
                model.SaveMessage = "Subscription saved.";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<SubscriptionsController>(
                   string.Format("Error saving subscription ({0}) with name: {1}", model.StripeId.IsSet() ? model.StripeId : "No Stripe Id", model.Name),
                   ex,
                   LogType.Error,
                   _userManager.GetUserId(User),
                   Url.AbsoluteAction("Index", "Subscriptions")
               );
            }
            return View(model);
        }

        [Route("admin/subscriptions/create/")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/subscriptions/add")]
        public async Task<Response> Add(CreateSubscriptionModel model)
        {
            Subscription subscription = new Subscription();
            try
            {
                ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);
                BillingSettings billingSettings = Engine.Settings.Billing;

                var newId = model.StripeId.IsSet() ? model.StripeId : Guid.NewGuid().ToString();
                if (Engine.Settings.Billing.EnableStripeTestMode)
                    newId += "-test";

                subscription = new Subscription
                {
                    CreatedBy = user.Id,
                    StripeId = newId,
                    Created = DateTime.Now,
                    LastEditedBy = user.UserName,
                    LastEditedOn = DateTime.Now,
                    Amount = (int)Math.Floor(model.Amount * 100),
                    Currency = model.Currency,
                    Description = model.Description,
                    Interval = model.Interval,
                    IntervalCount = model.IntervalCount,
                    Name = model.Name,
                    Public = model.Public,
                    Level = model.Level,
                    Addon = model.AddOn,
                    TrialPeriodDays = model.TrialPeriodDays,
                    LiveMode = billingSettings.EnableStripeTestMode
                };
                subscription = await _account.AddSubscriptionPlan(subscription);
                await _logService.AddLogAsync<SubscriptionsController>(
                    string.Format("Subscription ({0}) added with name: {1}", subscription.StripeId, subscription.Name),
                    JsonConvert.SerializeObject(subscription),
                    LogType.Success,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("Edit", "Subscriptions", new { id = subscription.Id })
                );

                var response = new Response(true, "Published successfully.");
                response.Url = Url.Action("Edit", new { id = subscription.Id, message = EditorMessage.Created });
                return response;
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<SubscriptionsController>(
                    string.Format("Error adding subscription ({0}) with name: {1}", subscription.StripeId.IsSet() ? subscription.StripeId : "No Stripe Id", subscription.Name),
                    ex,
                    LogType.Error,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("Index", "Subscriptions")
                );
                return new Response(ex.Message);
            }
        }

        [Route("admin/subscriptions/delete")]
        [HttpPost()]
        public async Task<Response> Delete(int id)
        {
            try
            {
                var subscription = await _account.GetSubscriptionPlanById(id);
                await _account.DeleteSubscriptionPlan(id);
                await _logService.AddLogAsync<SubscriptionsController>(
                    string.Format("Subscription ({0}) deleted with name: {1}", subscription.StripeId, subscription.Name),
                    JsonConvert.SerializeObject(subscription),
                    LogType.Success,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("Edit", "Subscriptions", new { id = subscription.Id })
                );
                return new Response(true);
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<SubscriptionsController>(
                    string.Format("Error deleting subscription with id: {0}", id),
                    ex,
                    LogType.Error,
                    _userManager.GetUserId(User),
                    Url.AbsoluteAction("Index", "Subscriptions")
                );
                return new Response(ex.Message);
            }
        }

        [Route("admin/subscriptions/stripe/")]
        public async Task<IActionResult> Stripe()
        {
            var model = await _billing.SubscriptionPlans.GetAllAsync();
            return View(model);
        }

        [Route("admin/subscriptions/stripe/test/webhook")]
        public IActionResult Webhook()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/subscriptions/stripe/test/webhook")]
        public IActionResult Webhook(StripeWebhookTest model)
        {
            string url = string.Format("{0}stripe/webhooks", ControllerContext.HttpContext.GetSiteUrl());
            try
            {
                if (!model.Body.IsSet())
                    throw new Exception("You must enter a body, in json format.");

                HttpWebRequest testRequest = (HttpWebRequest)WebRequest.Create(url);

                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(model.Body);

                testRequest.Method = "POST";
                testRequest.ContentType = "text/json"; //place MIME type here
                testRequest.ContentLength = data.Length;

                Stream newStream = testRequest.GetRequestStream();
                newStream.Write(data, 0, data.Length);
                newStream.Close();

                HttpWebResponse response = (HttpWebResponse)testRequest.GetResponse();

                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                {
                    model.Response = reader.ReadToEnd();
                }

                ViewBag.Message = string.Format(
                    "<div class='alert alert-success'><i class='fa fa-info-circle m-r-sm'></i>Remote server call to {0} was sent successfully.</div>",
                    url);
            }
            catch (WebException wex)
            {
                var httpResponse = wex.Response as HttpWebResponse;
                if (httpResponse != null)
                {
                    ViewBag.Message = string.Format(
                        "<div class='alert alert-danger'><i class='fa fa-exclamation-triangle m-r-sm'></i>Remote server call to {0} resulted in a http error {1} {2}.</div>",
                        url,
                        httpResponse.StatusCode,
                        httpResponse.StatusDescription);
                }
                else
                {
                    ViewBag.Message = string.Format(
                        "<div class='alert alert-danger'><i class='fa fa-exclamation-triangle m-r-sm'></i>Remote server call to {0} resulted in an error.</div>",
                        url);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = string.Format(
                    "<div class='alert alert-danger'><i class='fa fa-exclamation-triangle m-r-sm'></i>An error occurred: <strong>{0}</strong><br />{1}</div>",
                    ex.Message.ToString(),
                    ex.ToString());
            }
            return View(model);
        }

    }
}


