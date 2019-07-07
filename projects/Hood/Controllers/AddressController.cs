using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    [Authorize]
    public class AddressController : BaseController
    {
        public AddressController()
            : base()
        { }

        public ActionResult Index()
        {
            var user = _account.GetCurrentUserAsync(false);
            return View(user);
        }

        [HttpGet]
        public async Task<List<Address>> Get()
        {
            var user =await _account.GetCurrentUserAsync();
            return user.Addresses;
        }


        public ActionResult Create()
        {
            string userID = _userManager.GetUserId(User);
            Address add = new Address() { UserId = userID };
            return View(add);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Address address)
        {
            try
            {
                // Geocode
                var location = _address.GeocodeAddress(address);
                if (location != null)
                {
                    address.SetLocation(location.Coordinates);
                }

                var user = await _account.GetCurrentUserAsync(false);
                address.UserId = user.Id;
                user.Addresses.Add(address);
                await _account.UpdateUserAsync(user);

                if (user.BillingAddress == null)
                    user.BillingAddress = address.CloneTo<Address>();
                if (user.DeliveryAddress == null)
                    user.DeliveryAddress = address.CloneTo<Address>();

                await _account.UpdateUserAsync(user);

                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        public async Task<ActionResult> EditAsync(int id)
        {
            return View(await _account.GetAddressByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(Address address)
        {
            try
            {
                // Geocode
                var location = _address.GeocodeAddress(address);
                if (location != null)
                {
                    address.SetLocation(location.Coordinates);
                }

                await _account.UpdateAddressAsync(address);
                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        [HttpPost]
        public async Task<Response> DeleteAsync(int id)
        {
            try
            {
                await _account.DeleteAddressAsync(id);
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }

        [HttpPost]
        public async Task<Response> SetBillingAsync(int id)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                await _account.SetBillingAddressAsync(userId, id);
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }

        [HttpPost]
        public async Task<Response> SetDeliveryAsync(int id)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                await _account.SetDeliveryAddressAsync(userId, id);
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }
    }
}
