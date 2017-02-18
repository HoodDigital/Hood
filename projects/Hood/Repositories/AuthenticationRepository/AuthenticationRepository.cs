using Hood.Infrastructure;
using Hood.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using Hood.Interfaces;
using Hood.Caching;

namespace Hood.Services
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly HoodDbContext _db;
        private readonly IHttpContextAccessor _context;
        private readonly ISiteConfiguration _site;
        private readonly IHoodCache _cache;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBillingService _billing;

        public AuthenticationRepository(HoodDbContext db, ISiteConfiguration site, IBillingService billing, IHttpContextAccessor context, IHoodCache cache, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _site = site;
            _context = context;
            _cache = cache;
            _userManager = userManager;
            _billing = billing;
        }

        public AccountInfo GetAccountInfo(string userId)
        {
            AccountInfo result = new AccountInfo();
            result = new AccountInfo();
            result.User = GetUserById(userId);
            if (result.User?.Subscriptions != null)
            {
                foreach (UserSubscription sub in result.User.Subscriptions)
                {
                    if (sub.Status == "trialing" || sub.Status == "active")
                    {
                        result.ActiveSubscriptions.Add(new Models.Api.UserSubscriptionApi(sub));
                    }
                }
            }
            return result;
        }

        // Users
        public ApplicationUser GetUserById(string userId, bool cached = true, bool track = true)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            string cacheKey = typeof(ApplicationUser).ToString() + ".Single." + userId;
            ApplicationUser user;
            if (!_cache.TryGetValue(cacheKey, out user) || !cached)
            {
                var userQ = _db.Users.Include(u => u.Addresses)
                                       .Include(u => u.Subscriptions)
                                       .ThenInclude(u => u.Subscription)
                                       .Where(u => u.Id == userId);
                if (!track)
                    userQ = userQ.AsNoTracking();
                user = userQ.FirstOrDefault();
                _cache.Add(cacheKey, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));
            }
            return user;
        }
        public async Task<ApplicationUser> GetUserByStripeId(string stripeId)
        {
            if (string.IsNullOrEmpty(stripeId))
                return null;
            ApplicationUser user;
            user = await _db.Users.AsNoTracking()
                                      .Include(u => u.Addresses)
                                      .Include(u => u.Subscriptions)
                                      .ThenInclude(u => u.Subscription)
                                      .Where(u => u.StripeId == stripeId).FirstOrDefaultAsync();
            string cacheKey = typeof(ApplicationUser).ToString() + ".Single." + user.Id;
            _cache.Add(cacheKey, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));
            return user;
        }

        public void ClearUserFromCache(string userId)
        {
            _cache.Remove(typeof(ApplicationUser).ToString() + "-" + userId);
        }
        public void ClearUserFromCache(ApplicationUser user)
        {
            _cache.Remove(typeof(ApplicationUser).ToString() + "-" + user.Id);
        }

        public ApplicationUser GetCurrentUser(bool cached = true, bool track = true)
        {
            if (_context.HttpContext.User.Identity.IsAuthenticated)
                return GetUserById(_userManager.GetUserId(_context.HttpContext.User), cached, track);
            else
                return null;
        }

        public OperationResult UpdateUser(ApplicationUser user)
        {
            try
            {
                _db.Update(user);
                _db.SaveChanges();
                ClearUserFromCache(user);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }
        public void ResetBillingInfo()
        {
            var user = GetCurrentUser();
            user.StripeId = null;
            UpdateUser(user);
        }


        // Roles
        public IList<IdentityRole> GetAllRoles()
        {
            return _db.Roles.ToList();
        }

        // Addresses
        public Address GetAddressById(int id)
        {
            return _db.Addresses.Where(a => a.Id == id).FirstOrDefault();
        }
        public OperationResult DeleteAddress(int id)
        {
            try
            {
                Address address = _db.Addresses.Where(u => u.Id == id).FirstOrDefault();
                _db.Entry(address).State = EntityState.Deleted;
                OperationResult result = new OperationResult();
                _db.SaveChanges();
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
        public OperationResult UpdateAddress(Address address)
        {
            try
            {
                _db.Update(address);
                _db.SaveChanges();
                _cache.Remove(typeof(ApplicationUser).ToString() + "-" + address.UserId);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }
        public OperationResult SetBillingAddress(string userId, int id)
        {
            try
            {
                var user = _db.Users.Include(u => u.Addresses).Where(u => u.Id == userId).FirstOrDefault();
                var add = user.Addresses.SingleOrDefault(a => a.Id == id);
                if (add != null)
                    user.BillingAddress = add.CloneTo<Address>();
                _db.Update(user);
                _db.SaveChanges();
                _cache.Remove(typeof(ApplicationUser).ToString() + "-" + userId);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }
        public OperationResult SetDeliveryAddress(string userId, int id)
        {
            try
            {
                var user = _db.Users.Include(u => u.Addresses).Where(u => u.Id == userId).FirstOrDefault();
                var add = user.Addresses.SingleOrDefault(a => a.Id == id);
                if (add != null)
                    user.DeliveryAddress = add.CloneTo<Address>();
                _db.Update(user);
                _db.SaveChanges();
                _cache.Remove(typeof(ApplicationUser).ToString() + "-" + userId);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public OperationResult AddUserSubscription(UserSubscription newUserSub)
        {
            try
            {
                _db.UserSubscriptions.Add(newUserSub);
                _db.SaveChanges();
                ClearUserFromCache(newUserSub.UserId);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public OperationResult UpdateUserSubscription(UserSubscription newUserSub)
        {
            try
            {
                _db.Update(newUserSub);
                _db.SaveChanges();
                ClearUserFromCache(newUserSub.UserId);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }
    }
}
