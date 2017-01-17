using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using System.Linq;
using Hood.Services;
using Hood.Extensions;
using Hood.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using System;
using Newtonsoft.Json;
using Hood.Models.Api;
using System.Net;
using System.Text;
using System.IO;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class SubscriptionsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISubscriptionRepository _subscriptions;
        private readonly ISiteConfiguration _site;
        private readonly IHostingEnvironment _env;
        private readonly IBillingService _billing;

        public SubscriptionsController(
            ISubscriptionRepository subscriptions,
            UserManager<ApplicationUser> userManager,
            ISiteConfiguration site,
            IBillingService billing,
            IHostingEnvironment env)
        {
            _userManager = userManager;
            _subscriptions = subscriptions;
            _site = site;
            _billing = billing;
            _env = env;
        }

        [Route("admin/subscriptions/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("admin/subscribers/")]
        public async Task<IActionResult> Subscribers()
        {
            ViewBag.Subcriptions = await _subscriptions.GetAllAsync();
            return View();
        }

        [Route("admin/subscriptions/edit/{id}/")]
        public async Task<IActionResult> Edit(int id)
        {
            EditSubscriptionModel model = new EditSubscriptionModel();
            model.Subscription = await _subscriptions.GetSubscriptionById(id);       
            return View(model);
        }

        [HttpPost()]
        [Route("admin/subscriptions/edit/{id}/")]
        public async Task<ActionResult> Edit(Subscription model)
        {
            EditSubscriptionModel esm = new EditSubscriptionModel();
            esm.SaveResult = await _subscriptions.UpdateSubscription(model);
            esm.Subscription = model;
            return View(esm);
        }

        [Route("admin/subscriptions/create/")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        [Route("admin/subscriptions/get")]
        public async Task<JsonResult> Get(ListFilters request, string search, string sort)
        {
            PagedList<Subscription> subs = await _subscriptions.GetPagedSubscriptions(request, search, sort);
            Response response = new Response(subs.Items.Select(s => new SubscriptionApi(s)).ToArray(), subs.Count);
            return Json(response);
        }

        [HttpGet]
        [Route("admin/subscribers/get")]
        public async Task<JsonResult> GetSubscribers(ListFilters request, int subscriptionId, string search, string sort)
        {
            PagedList<ApplicationUser> subs = await _subscriptions.GetPagedSubscribers(request, subscriptionId, search, sort);
            Response response = new Response(subs.Items.Select(u => new ApplicationUserApi(u)).ToArray(), subs.Count);
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return Json(response, settings);
        }

        [HttpPost]
        [Route("admin/subscriptions/add")]
        public async Task<Response> Add(CreateSubscriptionModel model)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);
                BillingSettings billingSettings = _site.GetBillingSettings();
                Subscription subscription = new Subscription
                {
                    CreatedBy = user.Id,
                    StripeId = model.StripeId.IsSet() ? model.StripeId : Guid.NewGuid().ToString(),
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
                OperationResult result = await _subscriptions.Add(subscription);
                if (!result.Succeeded)
                {
                    throw new Exception(result.ErrorString);
                }

                return new Response(true);

            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/subscriptions/delete")]
        [HttpPost()]
        public async Task<Response> Delete(int id)
        {
            try
            {
                OperationResult result = await _subscriptions.Delete(id);
                if (result.Succeeded)
                {
                    return new Response(true);
                }
                else
                {
                    return new Response("There was a problem updating the database");
                }
            }
            catch (Exception ex)
            {
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


