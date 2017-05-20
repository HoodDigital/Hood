using Hood.Infrastructure;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Hood.Interfaces;
using Hood.Extensions;

namespace Hood.Areas.Hood.Controllers
{
    [Authorize]
    [Area("Hood")]
    public partial class AddressController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IContentRepository _content;
        private readonly IAccountRepository _auth;
        private readonly IAddressService _address;

        public AddressController(
            IContentRepository content,
            IAccountRepository auth,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IAddressService address)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AddressController>();
            _content = content;
            _auth = auth;
            _address = address;
        }

        [Route("account/addresses/")]
        public ActionResult Index()
        {
            var user = _auth.GetCurrentUser(false);
            return View(user);
        }

        [Route("account/addresses/create/")]
        public ActionResult Create()
        {
            string userID = _userManager.GetUserId(User);
            Address add = new Address() { UserId = userID };
            return View(add);
        }

        [HttpPost]
        [Route("account/addresses/create/")]
        public IActionResult Create(Address address)
        {
            try
            {
                // Geocode
                address.SetLocation(_address.GeocodeAddress(address));

                var user = _auth.GetCurrentUser(false);
                address.UserId = user.Id;
                user.Addresses.Add(address);
                _auth.UpdateUser(user);

                if (user.BillingAddress == null)
                    user.BillingAddress = address.CloneTo<Address>();
                if (user.DeliveryAddress == null)
                    user.DeliveryAddress = address.CloneTo<Address>();

                _auth.UpdateUser(user);

                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        [Route("account/addresses/edit/")]
        public ActionResult Edit(int id)
        {
            return View(_auth.GetAddressById(id));
        }

        [HttpPost]
        [Route("account/addresses/edit/")]
        public IActionResult Edit(Address address)
        {
            try
            {
                // Geocode
                address.SetLocation(_address.GeocodeAddress(address));

                OperationResult result = _auth.UpdateAddress(address);
                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        [HttpPost]
        [Route("account/addresses/delete/")]
        public ActionResult Delete(int id)
        {
            try
            {
                OperationResult result = _auth.DeleteAddress(id);
                if (result.Succeeded)
                    return Json(new { success = true });
                else
                    throw new Exception(result.ErrorString);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("account/addresses/setbilling/")]
        public ActionResult SetBilling(int id)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                OperationResult result = _auth.SetBillingAddress(userId, id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("account/addresses/setdelivery/")]
        public ActionResult SetDelivery(int id)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                OperationResult result = _auth.SetDeliveryAddress(userId, id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
