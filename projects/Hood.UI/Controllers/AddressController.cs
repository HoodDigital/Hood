using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Controllers
{

    [Authorize]
    public abstract class AddressController : AddressController<HoodDbContext>
    {
        public AddressController() : base() { }
    }

    [Authorize]
    public abstract class AddressController<TContext> : BaseController<TContext, ApplicationUser, IdentityRole>
         where TContext : HoodDbContext
    {
        public AddressController()
            : base()
        { }

        public virtual async Task<IActionResult> Index(AddressListModel model) => await List(model, "Index");
        [Route("account/address/list")]
        public virtual async Task<IActionResult> List(AddressListModel model, string viewName = "_List_Addresses")
        {
            IQueryable<Address> addresses = _db.Addresses;

            if (model.UserId.IsSet())
            {
                model.UserProfile = await _account.GetUserProfileByIdAsync(model.UserId);
                addresses = addresses.Where(a => a.UserId == model.UserId);
            }
            else
            {                
                model.UserProfile = await _account.GetUserProfileByIdAsync(User.GetLocalUserId());
                addresses = addresses.Where(a => a.UserId == User.GetLocalUserId());
            }

            if (!string.IsNullOrEmpty(model.Search))
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                addresses = addresses.Where(n => searchTerms.Any(s => n.QuickName != null && n.QuickName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Address1 != null && n.Address1.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Address2 != null && n.Address2.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.City != null && n.City.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Country != null && n.Country.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }

            if (!string.IsNullOrEmpty(model.Order))
            {
                switch (model.Order)
                {
                    case "name":
                    case "title":
                        addresses = addresses.OrderBy(n => n.QuickName);
                        break;

                    case "name+desc":
                    case "title+desc":
                        addresses = addresses.OrderByDescending(n => n.QuickName);
                        break;

                    default:
                        addresses = addresses.OrderByDescending(n => n.QuickName).ThenBy(n => n.Postcode);
                        break;
                }
            }

            await model.ReloadAsync(addresses);

            return View(viewName, model);
        }

        [Route("account/address/create")]
        public ActionResult Create(string userId)
        {
            if (!userId.IsSet())
                userId = User.GetLocalUserId();
            Address add = new Address() { UserId = userId };
            return View(add);
        }
        [HttpPost]
        [Route("account/address/create")]
        public virtual async Task<IActionResult> Create(Address address)
        {
            try
            {
                if (Engine.Settings.Integrations.IsGoogleGeocodingEnabled)
                {
                    try
                    {
                        var location = _address.GeocodeAddress(address);
                        if (location != null)
                        {
                            address.SetLocation(location.Coordinates);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logService.AddExceptionAsync<AddressController>("Error geocoding a user address.", ex);
                    }
                }

                var user = await _account.GetUserByIdAsync(User.GetLocalUserId());
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

        [Route("account/address/edit/{id}")]
        public virtual async Task<ActionResult> Edit(int id)
        {
            return View(await _account.GetAddressByIdAsync(id));
        }
        [HttpPost]
        [Route("account/address/edit/{id}")]
        public virtual async Task<IActionResult> Edit(Address address)
        {
            try
            {
                if (Engine.Settings.Integrations.IsGoogleGeocodingEnabled)
                {
                    try
                    {
                        var location = _address.GeocodeAddress(address);
                        if (location != null)
                        {
                            address.SetLocation(location.Coordinates);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logService.AddExceptionAsync<AddressController>("Error geocoding a user address.", ex);
                    }
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
        [Route("account/address/delete/{id}")]
        public virtual async Task<Response> Delete(int id)
        {
            try
            {
                await _account.DeleteAddressAsync(id);
                return new Response(true, $"The address has been deleted.");
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }

        [HttpPost]
        [Route("account/address/set-billing/{id}")]
        public virtual async Task<Response> SetBilling(int id)
        {
            try
            {
                string userId = User.GetLocalUserId();
                await _account.SetBillingAddressAsync(userId, id);
                return new Response(true, $"The billing address has been updated.");
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }
        [HttpPost]
        [Route("account/address/set-delivery/{id}")]
        public virtual async Task<Response> SetDelivery(int id)
        {
            try
            {
                string userId = User.GetLocalUserId();
                await _account.SetDeliveryAddressAsync(userId, id);
                return new Response(true, $"The delivery address has been updated.");
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }
    }
}
