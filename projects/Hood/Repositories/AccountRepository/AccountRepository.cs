using Hood.Caching;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class AccountRepository : IAccountRepository
    {
        private readonly HoodDbContext _db;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHoodCache _cache;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBillingService _billing;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountRepository(
            HoodDbContext db,
            IBillingService billing,
            IHttpContextAccessor context,
            IHoodCache cache,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _contextAccessor = context;
            _cache = cache;
            _userManager = userManager;
            _roleManager = roleManager;
            _billing = billing;
        }

        #region User Get/Update/Delete
        private IQueryable<ApplicationUser> UserQuery
        {
            get
            {
                var query = _db.Users
                    .Include(u => u.Addresses)
                    .Include(u => u.AccessCodes)
                    .Include(u => u.Subscriptions).ThenInclude(u => u.Subscription);
                return query;
            }
        }
        public async Task<ApplicationUser> GetUserByIdAsync(string id, bool track = true)
        {
            if (!id.IsSet())
                return null;
            var query = UserQuery;
            if (!track)
                query = query.AsNoTracking();
            return await query.SingleOrDefaultAsync(u => u.Id == id);
        }
        public async Task<ApplicationUser> GetUserByEmailAsync(string email, bool track = true)
        {
            if (!email.IsSet())
                return null;
            var query = UserQuery;
            if (!track)
                query = query.AsNoTracking();
            return await query.SingleOrDefaultAsync(u => u.Email == email);
        }
        public async Task<ApplicationUser> GetUserByStripeIdAsync(string stripeId, bool track = true)
        {
            if (!stripeId.IsSet())
                return null;
            var query = UserQuery;
            if (!track)
                query = query.AsNoTracking();
            return await query.SingleOrDefaultAsync(u => u.StripeId == stripeId);
        }
        public async Task<ApplicationUser> GetCurrentUserAsync(bool track = true)
        {
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
                return await GetUserByIdAsync(_userManager.GetUserId(_contextAccessor.HttpContext.User), track);
            else
                return null;
        }
        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _db.Update(user);
            await _db.SaveChangesAsync();
        }
        public async Task DeleteUserAsync(ApplicationUser user)
        {
            string container = typeof(ApplicationUser).Name;
            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var li in logins)
            {
                await _userManager.RemoveLoginAsync(user, li.LoginProvider, li.ProviderKey);
            }
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }
            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in claims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            var userSubs = await GetUserByIdAsync(user.Id);
            if (userSubs.Subscriptions != null)
            {
                if (userSubs.Subscriptions.Count > 0)
                {
                    foreach (var sub in userSubs.Subscriptions)
                    {
                        try
                        {
                            var res = await _billing.Subscriptions.CancelSubscriptionAsync(sub.CustomerId, sub.StripeId, false);
                        }
                        catch (Stripe.StripeException ex)
                        {
                            switch (ex.StripeError.ErrorType)
                            {
                                case "card_error":
                                case "api_connection_error":
                                case "api_error":
                                case "authentication_error":
                                case "invalid_request_error":
                                case "rate_limit_error":
                                case "validation_error":
                                    throw new Exception("Errorcancelling active subscriptions.");
                                default:
                                    break;
                            }
                        }
                    }
                    userSubs.Subscriptions.Clear();
                    await UpdateUserAsync(userSubs);
                }
            }
            await _db.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
            return;
        }
        #endregion

        #region Profiles 
        public async Task<UserListModel> GetUserProfilesAsync(UserListModel model)
        {
            var query = _db.UserProfiles.AsQueryable();

            if (model.Role.IsSet())
            {
                query = query.Where(q => q.RoleIds.Contains(model.Role));
            }

            if (model.Subscription.IsSet())
            {
                query = query.Where(q => q.ActiveSubscriptionIds.Contains(model.Subscription));
            }

            if (!string.IsNullOrEmpty(model.Search))
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(n => searchTerms.Any(s => n.UserName.ToLower().Contains(s.ToLower())));
            }

            switch (model.Order)
            {
                case "UserName":
                    query = query.OrderBy(n => n.UserName);
                    break;
                case "Email":
                    query = query.OrderBy(n => n.Email);
                    break;
                case "LastName":
                    query = query.OrderBy(n => n.LastName);
                    break;
                case "LastLogOn":
                    query = query.OrderByDescending(n => n.LastLogOn);
                    break;

                case "UserNameDesc":
                    query = query.OrderByDescending(n => n.UserName);
                    break;
                case "EmailDesc":
                    query = query.OrderByDescending(n => n.Email);
                    break;
                case "LastNameDesc":
                    query = query.OrderByDescending(n => n.LastName);
                    break;

                default:
                    query = query.OrderBy(n => n.UserName);
                    break;
            }

            await model.ReloadAsync(query);

            return model;
        }
        public async Task<UserProfile> GetProfileAsync(string id)
        {
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id);
            return profile;
        }
        public async Task<List<UserAccessCode>> GetAccessCodesAsync(string id)
        {
            return await _db.AccessCodes.Where(u => u.UserId == id).ToListAsync();
        }
        public async Task UpdateProfileAsync(UserProfile user)
        {
            var userToUpdate = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            foreach (PropertyInfo property in typeof(IUserProfile).GetProperties())
                property.SetValue(userToUpdate, property.GetValue(user));

            foreach (PropertyInfo property in typeof(IName).GetProperties())
                property.SetValue(userToUpdate, property.GetValue(user));

            foreach (PropertyInfo property in typeof(IAvatar).GetProperties())
                property.SetValue(userToUpdate, property.GetValue(user));

            foreach (PropertyInfo property in typeof(IJsonMetadata).GetProperties())
                property.SetValue(userToUpdate, property.GetValue(user));

            _db.Update(userToUpdate);
            _db.SaveChanges();
        }
        #endregion

        #region Roles
        public async Task<IList<IdentityRole>> GetAllRolesAsync()
        {
#warning TODO: Cache this method.
            return await _db.Roles.ToListAsync();
        }
        #endregion

        #region Addresses
        public async Task<Models.Address> GetAddressByIdAsync(int id)
        {
            return await _db.Addresses.Where(a => a.Id == id).FirstOrDefaultAsync();
        }
        public async Task DeleteAddressAsync(int id)
        {
            var address = GetAddressByIdAsync(id);
            _db.Entry(address).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
        }
        public async Task UpdateAddressAsync(Models.Address address)
        {
            _db.Update(address);
            await _db.SaveChangesAsync();
        }
        public async Task SetBillingAddressAsync(string userId, int id)
        {
            var user = await GetUserByIdAsync(userId);
            var add = user.Addresses.SingleOrDefault(a => a.Id == id);
            if (add != null)
                user.BillingAddress = add.CloneTo<Models.Address>();
            await UpdateUserAsync(user);
        }
        public async Task SetDeliveryAddressAsync(string userId, int id)
        {
            var user = await GetUserByIdAsync(userId);
            var add = user.Addresses.SingleOrDefault(a => a.Id == id);
            if (add != null)
                user.DeliveryAddress = add.CloneTo<Models.Address>();
            await UpdateUserAsync(user);
        }
        #endregion

        #region Subscriptions
        public async Task<SubscriptionSearchModel> GetSubscriptionPlansAsync(SubscriptionSearchModel model = null)
        {
            IQueryable<Models.Subscription> query = _db.Subscriptions.Include(s => s.Features);

            if (model == null)
                model = new SubscriptionSearchModel();

            // search the collection
            if (!string.IsNullOrEmpty(model.Search))
            {

                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(n => searchTerms.Any(s => n.Name != null && n.Name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Description != null && n.Description.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.StripeId != null && n.StripeId.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.StatementDescriptor != null && n.StatementDescriptor.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }


            // sort the collection and then output it.
            if (!string.IsNullOrEmpty(model.Order))
            {
                switch (model.Order)
                {
                    case "Title":
                        query = query.OrderBy(n => n.Name);
                        break;
                    case "Date":
                        query = query.OrderBy(n => n.Created);
                        break;
                    case "Price":
                        query = query.OrderBy(n => n.Amount);
                        break;

                    case "TitleDesc":
                        query = query.OrderByDescending(n => n.Name);
                        break;
                    case "DateDesc":
                        query = query.OrderByDescending(n => n.Created);
                        break;
                    case "PriceDesc":
                        query = query.OrderByDescending(n => n.Amount);
                        break;

                    default:
                        query = query.OrderByDescending(n => n.Created).ThenBy(n => n.Id);
                        break;
                }
            }

            await model.ReloadAsync(query);

            return model;
        }
        public async Task<Models.Subscription> GetSubscriptionPlanByIdAsync(int id)
        {
            Models.Subscription subscription = await _db.Subscriptions
                                    .Include(s => s.Users)
                                    .Include(s => s.Features)
                                    .FirstOrDefaultAsync(c => c.Id == id);
            return subscription;
        }
        public async Task<Models.Subscription> GetSubscriptionPlanByStripeIdAsync(string stripeId)
        {
            Models.Subscription subscription = await _db.Subscriptions
                                .Include(s => s.Users)
                                .Include(s => s.Features)
                                .FirstOrDefaultAsync(c => c.StripeId == stripeId);

            return subscription;
        }
        public async Task<Models.Subscription> AddSubscriptionPlanAsync(Models.Subscription subscription)
        {
            // try adding to stripe
            var myPlan = new PlanCreateOptions()
            {
                Id = subscription.StripeId,
                Amount = subscription.Amount,                     // all amounts on Stripe are in cents, pence, etc
                Currency = subscription.Currency,                 // "usd" only supported right now
                Interval = subscription.Interval,                 // "month" or "year"
                IntervalCount = subscription.IntervalCount,       // optional
                TrialPeriodDays = subscription.TrialPeriodDays,   // amount of time that will lapse before the customer is billed
                Product = new PlanProductCreateOptions()
                {
                    Name = subscription.Name
                }
            };
            Plan response = await _billing.Stripe.PlanService.CreateAsync(myPlan);
            subscription.StripeId = response.Id;
            _db.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync();
            return subscription;
        }
        public async Task UpdateSubscriptionAsync(Models.Subscription subscription)
        {
            var myPlan = new PlanUpdateOptions()
            {
                Nickname = subscription.Name
            };
            Plan response = await _billing.Stripe.PlanService.UpdateAsync(subscription.StripeId, myPlan);
            _db.Update(subscription);
            await _db.SaveChangesAsync();
            return;
        }
        public async Task DeleteSubscriptionPlanAsync(int id)
        {
            Models.Subscription subscription = _db.Subscriptions.Where(p => p.Id == id).FirstOrDefault();
            try
            {
                await _billing.Stripe.PlanService.DeleteAsync(subscription.StripeId);
            }
            catch (StripeException)
            { }
            _db.SaveChanges();
            _db.Entry(subscription).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            return;
        }
        #endregion

        #region Stripe customer object
        public async Task<Customer> GetCustomerObjectAsync(string stripeId)
        {
            // Check if the user has a stripeId - if they do, we dont need to create them again, we can simply add a new card token to their account, or use an existing one maybe.
            if (!stripeId.IsSet())
                return null;
            try
            {
                var customer = await _billing.Customers.FindByIdAsync(stripeId);
                return customer;
            }
            catch (StripeException)
            {
                return null;
            }
        }
        public async Task<List<Customer>> GetMatchingCustomerObjectsAsync(string email)
        {
            // Check if the user has a stripeId - if they do, we dont need to create them again, we can simply add a new card token to their account, or use an existing one maybe.
            if (email.IsSet())
                try
                {
                    var customers = await _billing.Customers.GetAsync(email);
                    return customers.ToList();
                }
                catch (StripeException)
                {
                    return new List<Customer>();
                }
            else
                return new List<Customer>();
        }
        private async Task ResetBillingInfoAsync()
        {
            var user = await GetCurrentUserAsync();
            user.StripeId = null;
            await UpdateUserAsync(user);
        }
        #endregion

        #region User Subscriptions
        public async Task<UserSubscriptionListModel> GetUserSubscriptionsAsync(UserSubscriptionListModel model)
        {
            IQueryable<ApplicationUser> users = _db.Users
                .Include(u => u.Subscriptions).ThenInclude(u => u.Subscription);

            if (model.Subscription.IsSet())
                users = users.Where(u => u.Subscriptions.Any(s => s.Subscription.StripeId == model.Subscription && (s.Status == "trialing" || s.Status == "active")));
            else
                users = users.Where(u => u.Subscriptions.Any(s => s.Status == "trialing" || s.Status == "active"));

            // search the collection
            if (!string.IsNullOrEmpty(model.Search))
            {

                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                users = users.Where(n => searchTerms.Any(s => n.FirstName != null && n.FirstName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.LastName != null && n.LastName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.StripeId != null && n.StripeId.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Email != null && n.Email.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }


            // sort the collection and then output it.
            if (!string.IsNullOrEmpty(model.Order))
            {
                switch (model.Order)
                {
                    case "UserName":
                        users = users.OrderBy(n => n.UserName);
                        break;
                    case "Email":
                        users = users.OrderBy(n => n.Email);
                        break;
                    case "LastName":
                        users = users.OrderBy(n => n.LastName);
                        break;
                    case "LastLogOn":
                        users = users.OrderByDescending(n => n.LastLogOn);
                        break;

                    case "UserNameDesc":
                        users = users.OrderByDescending(n => n.UserName);
                        break;
                    case "EmailDesc":
                        users = users.OrderByDescending(n => n.Email);
                        break;
                    case "LastNameDesc":
                        users = users.OrderByDescending(n => n.LastName);
                        break;

                    default:
                        users = users.OrderByDescending(n => n.CreatedOn).ThenBy(n => n.Id);
                        break;
                }
            }
            await model.ReloadAsync(users);
            return model;
        }
        public async Task<UserSubscription> CreateUserSubscriptionAsync(int planId, string stripeToken, string cardId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            // Load the subscription plan.
            Models.Subscription subscription = await GetSubscriptionPlanByIdAsync(planId);

            // Get the stripe subscription plan object.
            Plan plan = await _billing.SubscriptionPlans.FindByIdAsync(subscription.StripeId);

            // Check for customer or throw, but allow it to be null.
            var customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
                throw new Exception("There was a problem loading the customer object.");

            // The object for the new user subscription to be recieved from stripe.
            Stripe.Subscription newSubscription = null;

            // if the user has provided a cardId to use, then let's try and use that!
            if (cardId.IsSet())
            {
                if (customer == null)
                    throw new Exception("There is no customer account associated with this user.");

                // set the card as the default for the user, then subscribe the user.
                await _billing.Customers.SetDefaultCard(customer.Id, cardId);

                // check if the user is already on a subscription, if so, update that.
                var sub = (await _billing.Subscriptions.UserSubscriptionsAsync(user.StripeId)).FirstOrDefault(s => s.Plan.Id == plan.Id && s.Status != "canceled");
                if (sub != null)
                {
                    // there is an existing subscription, which is not cancelleed and matches the plan. BAIL OUT!                   
                    throw new Exception("There is already a matching active subscription to this plan.");
                }
                else
                {
                    // finally, add the user to the NEW subscription.
                    newSubscription = await _billing.Subscriptions.SubscribeUserAsync(customer.Id, subscription.StripeId);
                }
            }
            else
            {
                // if not, then the user must have supplied a token
                Stripe.Token stripeTokenObj = _billing.Stripe.TokenService.Get(stripeToken);
                if (stripeTokenObj == null)
                    throw new Exception("The payment method token was invalid.");

                // Check if the customer object exists.
                if (customer == null)
                {
                    // if it does not, create it, add the card and subscribe the plan.
                    customer = await _billing.Customers.CreateCustomer(user, stripeToken);
                    user.StripeId = customer.Id;
                    await UpdateUserAsync(user);
                    newSubscription = await _billing.Subscriptions.SubscribeUserAsync(user.StripeId, plan.Id);
                }
                else
                {
                    // check if the user is already on a subscription, if so, update that.
                    var sub = (await _billing.Subscriptions.UserSubscriptionsAsync(user.StripeId)).FirstOrDefault(s => s.Plan.Id == plan.Id && s.Status != "canceled");
                    if (sub != null)
                    {
                        // there is an existing subscription, which is not cancelleed and matches the plan. BAIL OUT!                   
                        throw new Exception("There is already a matching active subscription to this plan.");
                    }
                    else
                    {
                        // finally, add the user to the NEW subscription, using the new card as the charge source.
                        var source = await _billing.Cards.CreateCard(customer.Id, stripeToken);

                        // set the card as the default for the user, then subscribe the user.
                        await _billing.Customers.SetDefaultCard(customer.Id, source.Id);

                        newSubscription = await _billing.Subscriptions.SubscribeUserAsync(customer.Id, plan.Id);
                    }
                }
            }

            // We got this far, let's add the detail to the local DB.
            if (newSubscription == null)
                throw new Exception("The new subscription was not created correctly, please try again.");

            UserSubscription newUserSub = new UserSubscription();
            newUserSub = newUserSub.UpdateFromStripe(newSubscription);
            newUserSub.StripeId = newSubscription.Id;
            newUserSub.CustomerId = user.StripeId;
            newUserSub.UserId = user.Id;
            newUserSub.SubscriptionId = subscription.Id;
            await _db.UserSubscriptions.AddAsync(newUserSub);
            _db.SaveChanges();
            return newUserSub;
        }
        public async Task<UserSubscription> UpdateUserSubscriptionAsync(UserSubscription newUserSub)
        {
            _db.Entry(newUserSub).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return newUserSub;
        }
        public async Task<UserSubscription> UpgradeUserSubscriptionAsync(int subscriptionId, int planId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            Models.Subscription subscription = await GetSubscriptionPlanByIdAsync(planId);
            UserSubscription userSub = GetUserSubscription(user, subscriptionId);

            // Check for customer or throw.
            var customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
                throw new Exception("There was a problem loading the customer object.");

            if (customer.DefaultSourceId.IsSet() || customer.Sources?.Count() > 0)
            {
                // there is a payment source - continue                  
                // Load the plan from stripe, then add to the user's subscription.
                Stripe.Plan plan = await _billing.SubscriptionPlans.FindByIdAsync(subscription.StripeId);
                Stripe.Subscription sub = await _billing.Subscriptions.UpdateSubscriptionAsync(customer.Id, userSub.StripeId, plan);
                userSub = userSub.UpdateFromStripe(sub);
                userSub.Subscription = subscription;
                userSub.SubscriptionId = subscription.Id;
                await UpdateUserAsync(user);
            }
            else
                throw new Exception("There is no currently active payment source on the account, please add one before upgrading.");

            return userSub;
        }
        public async Task<UserSubscription> CancelUserSubscriptionAsync(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            // Check for customer or throw.
            var customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
                throw new Exception("There was a problem loading the customer object.");

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _billing.Subscriptions.CancelSubscriptionAsync(customer.Id, userSub.StripeId, true);
            userSub = userSub.UpdateFromStripe(sub);
            await UpdateUserAsync(user);
            return userSub;
        }
        public async Task<UserSubscription> RemoveUserSubscriptionAsync(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            // Check for customer or throw.
            var customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
                throw new Exception("There was a problem loading the customer object.");

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _billing.Subscriptions.CancelSubscriptionAsync(customer.Id, userSub.StripeId, false);
            userSub = userSub.UpdateFromStripe(sub);
            await UpdateUserAsync(user);
            return userSub;
        }
        public async Task<UserSubscription> ReactivateUserSubscriptionAsync(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            // Check for customer or throw.
            var customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
                throw new Exception("There was a problem loading the customer object.");

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _billing.Subscriptions.FindById(user.StripeId, userSub.StripeId);

            sub = await _billing.Subscriptions.UpdateSubscriptionAsync(customer.Id, sub.Id, sub.Plan);
            userSub = userSub.UpdateFromStripe(sub);
            await UpdateUserAsync(user);
            return userSub;
        }
        #endregion

        #region UserSubscription Helpers
        private UserSubscription GetUserSubscription(ApplicationUser user, int userSubscriptionId)
        {
            UserSubscription subscription = user.Subscriptions.Where(us => us.Id == userSubscriptionId).FirstOrDefault();
            if (subscription == null)
                throw new Exception("Could not load the subscription for the user.");
            return subscription;
        }
        private UserSubscription GetUserSubscriptionByStripeId(ApplicationUser user, string stripeId)
        {
            UserSubscription subscription = user.Subscriptions.Where(us => us.StripeId == stripeId).FirstOrDefault();
            if (subscription == null)
                throw new Exception("Could not load the subscription for the user.");
            return subscription;
        }
        #endregion

        #region WebHooks
        public async Task ConfirmSubscriptionObjectAsync(Stripe.Subscription created, DateTime? eventTime)
        {
            ApplicationUser user = await GetUserByStripeIdAsync(created.CustomerId);
            if (user == null)
                throw new Exception($"Could not locate user from Stripe id: {created.CustomerId}");

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, created.Id);
            if (userSub == null)
                throw new Exception($"Could not locate user's subscription from Stripe id: {created.Id}");

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.
            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
                return;

            userSub = userSub.UpdateFromStripe(created);
            userSub.Confirmed = true;
            userSub.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        public async Task UpdateSubscriptionObjectAsync(Stripe.Subscription updated, DateTime? eventTime)
        {
            ApplicationUser user = await GetUserByStripeIdAsync(updated.CustomerId);
            if (user == null)
                throw new Exception($"Could not locate user from Stripe id: {updated.CustomerId}");

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, updated.Id);
            if (userSub == null)
                throw new Exception($"Could not locate user's subscription from Stripe id: {updated.Id}");

            Models.Subscription plan = await GetSubscriptionPlanByStripeIdAsync(updated.Plan.Id);
            if (plan == null)
                throw new Exception($"Could not locate subscription plan object from Stripe id: {updated.Plan.Id}");

            if (userSub.SubscriptionId != plan.Id)
            {
                userSub.SubscriptionId = plan.Id;
            }

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.

            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
                return;

            userSub = userSub.UpdateFromStripe(updated);
            userSub.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        public async Task RemoveUserSubscriptionObjectAsync(Stripe.Subscription deleted, DateTime? eventTime)
        {
            ApplicationUser user = await GetUserByStripeIdAsync(deleted.CustomerId);
            if (user == null)
                throw new Exception($"Could not locate user from Stripe id: {deleted.CustomerId}");

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, deleted.Id);
            if (userSub == null)
                throw new Exception($"Could not locate user's subscription from Stripe id: {deleted.Id}");

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.
            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
                return;

            userSub = userSub.UpdateFromStripe(deleted);
            userSub.Deleted = true;
            userSub.DeletedAt = DateTime.Now;
            userSub.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        public async Task<UserSubscription> GetUserSubscriptionByStripeIdAsync(string id)
        {
            return await _db.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Subscription)
                .SingleOrDefaultAsync(c => c.StripeId == id);
        }
        #endregion

        #region Statistics
        public async Task<object> GetStatisticsAsync()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
            var data = await _db.Users.Select(c => new { date = c.CreatedOn.Date, month = c.CreatedOn.Month }).ToListAsync();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });

            var days = new List<KeyValuePair<string, int>>();
            foreach (DateTime day in DateTimeExtensions.EachDay(DateTime.Now.AddDays(-89), DateTime.Now))
            {
                var dayvalue = createdByDate.SingleOrDefault(c => c.name == day.Date);
                var count = dayvalue != null ? dayvalue.count : 0;
                days.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));

            }

            var months = new List<KeyValuePair<string, int>>();
            for (DateTime dt = DateTime.Now.AddMonths(-11); dt <= DateTime.Now; dt = dt.AddMonths(1))
            {
                var monthvalue = createdByMonth.SingleOrDefault(c => c.name == dt.Month);
                var count = monthvalue != null ? monthvalue.count : 0;
                months.Add(new KeyValuePair<string, int>(dt.ToString("dd MMM"), count));
            }

            return new { totalUsers, totalAdmins, days, months };
        }
        public async Task<object> GetSubscriptionStatisticsAsync()
        {
            var total = await _db.UserSubscriptions.CountAsync();
            var trials = await _db.UserSubscriptions.Where(c => c.Status == "trialing").CountAsync();
            var active = await _db.UserSubscriptions.Where(c => c.Status == "active").CountAsync();

            var data = await _db.UserSubscriptions.Where(c => c.Created.HasValue).Select(c => new { date = c.Created.Value.Date, month = c.Created.Value.Month }).ToListAsync();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });

            var days = new List<KeyValuePair<string, int>>();
            foreach (DateTime day in DateTimeExtensions.EachDay(DateTime.Now.AddDays(-89), DateTime.Now))
            {
                var dayvalue = createdByDate.SingleOrDefault(c => c.name == day.Date);
                var count = dayvalue != null ? dayvalue.count : 0;
                days.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));

            }

            var months = new List<KeyValuePair<string, int>>();
            for (DateTime dt = DateTime.Now.AddMonths(-11); dt <= DateTime.Now; dt = dt.AddMonths(1))
            {
                var monthvalue = createdByMonth.SingleOrDefault(c => c.name == dt.Month);
                var count = monthvalue != null ? monthvalue.count : 0;
                months.Add(new KeyValuePair<string, int>(dt.ToString("MMMM, yyyy"), count));
            }

            return new { total, trials, active, days, months };
        }
        #endregion

        #region Obsolete

        [Obsolete("Use _userManager.GetUserSubscriptionView(ClaimsPrincipal principal) from now on.", true)]
        public AccountInfo LoadAccountInfo(string userId) => throw new NotImplementedException();

        #endregion
    }


}
