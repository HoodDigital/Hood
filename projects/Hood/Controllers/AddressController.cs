using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Hood.Controllers
{
    [Authorize]
    public class AddressController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public AddressController()
        { }

        public ActionResult Index()
        {
            var user = _account.GetCurrentUser(false);
            return View(user);
        }

        public ActionResult Create()
        {
            string userID = _userManager.GetUserId(User);
            Address add = new Address() { UserId = userID };
            return View(add);
        }

        [HttpPost]
        public IActionResult Create(Address address)
        {
            try
            {
                // Geocode
                var location = _address.GeocodeAddress(address);
                if (location != null)
                {
                    address.SetLocation(location.Coordinates);
                }

                var user = _account.GetCurrentUser(false);
                address.UserId = user.Id;
                user.Addresses.Add(address);
                _account.UpdateUser(user);

                if (user.BillingAddress == null)
                    user.BillingAddress = address.CloneTo<Address>();
                if (user.DeliveryAddress == null)
                    user.DeliveryAddress = address.CloneTo<Address>();

                _account.UpdateUser(user);

                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        public ActionResult Edit(int id)
        {
            return View(_account.GetAddressById(id));
        }

        [HttpPost]
        public IActionResult Edit(Address address)
        {
            try
            {
                // Geocode
                var location = _address.GeocodeAddress(address);
                if (location != null)
                {
                    address.SetLocation(location.Coordinates);
                }

                OperationResult result = _account.UpdateAddress(address);
                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                OperationResult result = _account.DeleteAddress(id);
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
        public ActionResult SetBilling(int id)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                OperationResult result = _account.SetBillingAddress(userId, id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SetDelivery(int id)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                OperationResult result = _account.SetDeliveryAddress(userId, id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
