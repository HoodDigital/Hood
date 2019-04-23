using Hood.Caching;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class AccountRepository : IAccountRepository
    {
        private readonly HoodDbContext _db;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISettingsRepository _settings;
        private readonly IHoodCache _cache;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBillingService _billing;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountRepository(
            HoodDbContext db,
            ISettingsRepository site,
            IBillingService billing,
            IHttpContextAccessor context,
            IHoodCache cache,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _settings = site;
            _contextAccessor = context;
            _cache = cache;
            _userManager = userManager;
            _roleManager = roleManager;
            _billing = billing;
        }

        // Users
        public AccountInfo LoadAccountInfo(string userId)
        {
            AccountInfo result = new AccountInfo();
            result = new AccountInfo()
            {
                User = GetUserById(userId, false)
            };
            if (result.User != null)
            {
                result.Roles = _userManager.GetRolesAsync(result.User).Result;
            }
            if (result.User?.Subscriptions != null)
            {
                foreach (UserSubscription sub in result.User.Subscriptions)
                {
                    if (sub.Status == "trialing" || sub.Status == "active")
                    {
                        result.ActiveSubscriptions.Add(sub);
                    }
                }
            }
            return result;
        }
        public ApplicationUser GetUserById(string userId, bool track = true)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            ApplicationUser user;
            var userQ = _db.Users.Include(u => u.Addresses)
                                   .Include(u => u.AccessCodes)
                                   .Include(u => u.Subscriptions)
                                   .ThenInclude(u => u.Subscription)
                                   .Where(u => u.Id == userId);
            if (!track)
                userQ = userQ.AsNoTracking();
            user = userQ.FirstOrDefault();
            return user;
        }
        public ApplicationUser GetUserByEmail(string email, bool track = true)
        {
            if (string.IsNullOrEmpty(email))
                return null;
            ApplicationUser user;
            var userQ = _db.Users.Include(u => u.Addresses)
                                   .Include(u => u.AccessCodes)
                                   .Include(u => u.Subscriptions)
                                   .ThenInclude(u => u.Subscription)
                                   .Where(u => u.Email == email);
            if (!track)
                userQ = userQ.AsNoTracking();
            user = userQ.FirstOrDefault();
            return user;
        }
        public async Task<ApplicationUser> GetUserByStripeId(string stripeId)
        {
            if (string.IsNullOrEmpty(stripeId))
                return null;
            ApplicationUser user;
            user = await _db.Users.Include(u => u.Addresses)
                                     .Include(u => u.AccessCodes)
                                      .Include(u => u.Subscriptions)
                                      .ThenInclude(u => u.Subscription)
                                      .Where(u => u.StripeId == stripeId).FirstOrDefaultAsync();
            return user;
        }
        public ApplicationUser GetCurrentUser(bool track = true)
        {
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
                return GetUserById(_userManager.GetUserId(_contextAccessor.HttpContext.User), track);
            else
                return null;
        }
        public OperationResult UpdateUser(ApplicationUser user)
        {
            try
            {
                _db.Update(user);
                _db.SaveChanges();
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
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

            var userSubs = GetUserById(user.Id);
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
                                    throw new Exception("An error occurred while cancelling active subscriptions.");
                                default:
                                    break;
                            }
                        }
                    }
                    userSubs.Subscriptions.Clear();
                    UpdateUser(userSubs);
                }
            }
            await _db.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
            return;
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
        public Models.Address GetAddressById(int id)
        {
            return _db.Addresses.Where(a => a.Id == id).FirstOrDefault();
        }
        public OperationResult DeleteAddress(int id)
        {
            try
            {
                Models.Address address = _db.Addresses.Where(u => u.Id == id).FirstOrDefault();
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
        public OperationResult UpdateAddress(Models.Address address)
        {
            try
            {
                _db.Update(address);
                _db.SaveChanges();
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
                    user.BillingAddress = add.CloneTo<Models.Address>();
                _db.Update(user);
                _db.SaveChanges();
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
                    user.DeliveryAddress = add.CloneTo<Models.Address>();
                _db.Update(user);
                _db.SaveChanges();
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }
        public async Task<UserSubscription> SaveUserSubscription(UserSubscription newUserSub)
        {
            await _db.UserSubscriptions.AddAsync(newUserSub);
            _db.SaveChanges();
            return newUserSub;
        }
        public async Task<UserSubscription> UpdateUserSubscription(UserSubscription newUserSub)
        {
            _db.Entry(newUserSub).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return newUserSub;
        }

        // Subscriptions
        public async Task<Models.Subscription> AddSubscriptionPlan(Models.Subscription subscription)
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
        public async Task DeleteSubscriptionPlan(int id)
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
        public async Task<Models.Subscription> GetSubscriptionPlanById(int id)
        {
            Models.Subscription subscription = await _db.Subscriptions
                                    .Include(s => s.Users)
                                    .Include(s => s.Features)
                                    .FirstOrDefaultAsync(c => c.Id == id);
            return subscription;
        }
        public async Task<Models.Subscription> GetSubscriptionPlanByStripeId(string stripeId)
        {
            Models.Subscription subscription = await _db.Subscriptions
                                .Include(s => s.Users)
                                .Include(s => s.Features)
                                .FirstOrDefaultAsync(c => c.StripeId == stripeId);

            return subscription;
        }
        private IQueryable<Models.Subscription> GetSubscriptionPlans(string search = "", string sort = "", bool includeUsers = false)
        {
            IQueryable<Models.Subscription> subscriptions = _db.Subscriptions.Include(s => s.Features);

            if (includeUsers)
                subscriptions = subscriptions.Include(s => s.Users);

            // search the collection
            if (!string.IsNullOrEmpty(search))
            {

                string[] searchTerms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                subscriptions = subscriptions.Where(n => searchTerms.Any(s => n.Name != null && n.Name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Description != null && n.Description.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.StripeId != null && n.StripeId.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.StatementDescriptor != null && n.StatementDescriptor.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }


            // sort the collection and then output it.
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "title":
                        subscriptions = subscriptions.OrderBy(n => n.Name);
                        break;
                    case "date":
                        subscriptions = subscriptions.OrderBy(n => n.Created);
                        break;
                    case "price":
                        subscriptions = subscriptions.OrderBy(n => n.Amount);
                        break;

                    case "title+desc":
                        subscriptions = subscriptions.OrderByDescending(n => n.Name);
                        break;
                    case "date+desc":
                        subscriptions = subscriptions.OrderByDescending(n => n.Created);
                        break;
                    case "price+desc":
                        subscriptions = subscriptions.OrderByDescending(n => n.Amount);
                        break;

                    default:
                        subscriptions = subscriptions.OrderByDescending(n => n.Created).ThenBy(n => n.Id);
                        break;
                }
            }
            return subscriptions;
        }
        public async Task<List<Models.Subscription>> GetSubscriptionPlansAsync()
        {
            return await GetSubscriptionPlans().ToListAsync();
        }
        public async Task<List<Models.Subscription>> GetSubscriptionPlanLevels(string category = null)
        {
            var subs = GetSubscriptionPlans().Where(s => s.Public && !s.Addon);
            if (category.IsSet())
            {
                subs = subs.Where(s => s.Category == category);
            }
            return await subs.OrderBy(s => s.Level).ToListAsync();
        }
        public async Task<List<Models.Subscription>> GetSubscriptionPlanAddons()
        {
            return await GetSubscriptionPlans().Where(s => s.Public && s.Addon).ToListAsync();
        }
        public async Task<SubscriptionSearchModel> GetPagedSubscriptionPlans(SubscriptionSearchModel model)
        {
            var subs = await GetSubscriptionPlans(model.Search, model.Order, true).ToListAsync();
            model.List = subs;
            return model;
        }
        public async Task UpdateSubscription(Models.Subscription subscription)
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

        // Stripe customer object
        public async Task<Customer> LoadCustomerObject(string stripeId, bool allowNullObject)
        {
            // Check if the user has a stripeId - if they do, we dont need to create them again, we can simply add a new card token to their account, or use an existing one maybe.
            if (stripeId.IsSet())
                try
                {
                    var customer = await _billing.Customers.FindByIdAsync(stripeId);
                    if (customer.Deleted.HasValue && customer.Deleted.Value)
                    {
                        ResetBillingInfo();
                        customer = null;
                    }
                    if (customer == null)
                        if (!allowNullObject)
                            throw new Exception(BillingMessage.NoCustomerObject.ToString());
                        else
                            ResetBillingInfo();
                    return customer;
                }
                catch (StripeException)
                {
                    if (allowNullObject)
                        return null;
                    else
                        throw new Exception(BillingMessage.NoCustomerObject.ToString());
                }
            else
                if (allowNullObject)
                return null;
            else
                throw new Exception(BillingMessage.NoStripeId.ToString());
        }

        // User Subscriptions
        private IQueryable<ApplicationUser> GetSubscribers(string subcription, string search = "", string sort = "")
        {
            IQueryable<ApplicationUser> users = _db.Users
                .Include(u => u.Subscriptions).ThenInclude(u => u.Subscription);

            if (subcription.IsSet())
                users = users.Where(u => u.Subscriptions.Any(s => s.Subscription.StripeId == subcription && (s.Status == "trialing" || s.Status == "active")));
            else
                users = users.Where(u => u.Subscriptions.Any(s => s.Status == "trialing" || s.Status == "active"));

            // search the collection
            if (!string.IsNullOrEmpty(search))
            {

                string[] searchTerms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                users = users.Where(n => searchTerms.Any(s => n.FirstName != null && n.FirstName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.LastName != null && n.LastName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.StripeId != null && n.StripeId.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Email != null && n.Email.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }


            // sort the collection and then output it.
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
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
            return users;
        }
        public async Task<SubscriberSearchModel> GetPagedSubscribers(SubscriberSearchModel model)
        {
            var subs = await GetSubscribers(model.Subscription, model.Search, model.Order).ToListAsync();
            model.List = subs;
            return model;
        }
        public async Task<UserSubscription> CreateUserSubscription(int planId, string stripeToken, string cardId)
        {
            // Load user object and clear cache.
            ApplicationUser user = GetCurrentUser();

            // Load the subscription plan.
            Models.Subscription subscription = await GetSubscriptionPlanById(planId);

            // Get the stripe subscription plan object.
            Plan plan = await _billing.SubscriptionPlans.FindByIdAsync(subscription.StripeId);

            // Check for customer or throw, but allow it to be null.
            Customer customer = await LoadCustomerObject(user.StripeId, true);

            // The object for the new user subscription to be recieved from stripe.
            Stripe.Subscription newSubscription = null;

            // if the user has provided a cardId to use, then let's try and use that!
            if (cardId.IsSet())
            {
                if (customer == null)
                    throw new Exception(BillingMessage.NoCustomerObject.ToString());

                // set the card as the default for the user, then subscribe the user.
                await _billing.Customers.SetDefaultCard(customer.Id, cardId);

                // check if the user is already on a subscription, if so, update that.
                var sub = (await _billing.Subscriptions.UserSubscriptionsAsync(user.StripeId)).FirstOrDefault(s => s.Plan.Id == plan.Id && s.Status != "canceled");
                if (sub != null)
                {
                    // there is an existing subscription, which is not cancelleed and matches the plan. BAIL OUT!                   
                    throw new Exception(BillingMessage.SubscriptionExists.ToString());
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
                    throw new Exception(BillingMessage.InvalidToken.ToString());

                // Check if the customer object exists.
                if (customer == null)
                {
                    // if it does not, create it, add the card and subscribe the plan.
                    customer = await _billing.Customers.CreateCustomer(user, stripeToken);
                    user.StripeId = customer.Id;
                    var updateResult = UpdateUser(user);
                    newSubscription = await _billing.Subscriptions.SubscribeUserAsync(user.StripeId, plan.Id);
                }
                else
                {
                    // check if the user is already on a subscription, if so, update that.
                    var sub = (await _billing.Subscriptions.UserSubscriptionsAsync(user.StripeId)).FirstOrDefault(s => s.Plan.Id == plan.Id && s.Status != "canceled");
                    if (sub != null)
                    {
                        // there is an existing subscription, which is not cancelleed and matches the plan. BAIL OUT!                   
                        throw new Exception(BillingMessage.SubscriptionExists.ToString());
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
                throw new Exception(BillingMessage.Error.ToString());

            UserSubscription newUserSub = new UserSubscription();
            newUserSub = UpdateUserSubscriptionFromStripe(newUserSub, newSubscription);
            newUserSub.StripeId = newSubscription.Id;
            newUserSub.CustomerId = user.StripeId;
            newUserSub.UserId = user.Id;
            newUserSub.SubscriptionId = subscription.Id;
            newUserSub = await SaveUserSubscription(newUserSub);
            return newUserSub;
        }
        public async Task<UserSubscription> UpgradeUserSubscription(int subscriptionId, int planId)
        {
            // Load user object and clear cache.
            ApplicationUser user = GetCurrentUser();

            Models.Subscription subscription = await GetSubscriptionPlanById(planId);
            UserSubscription userSub = GetUserSubscription(user, subscriptionId);

            // Check for customer or throw.
            var customer = await LoadCustomerObject(user.StripeId, false);

            if (customer.DefaultSourceId.IsSet() || customer.Sources?.Count() > 0)
            {
                // there is a payment source - continue                  
                // Load the plan from stripe, then add to the user's subscription.
                Stripe.Plan plan = await _billing.SubscriptionPlans.FindByIdAsync(subscription.StripeId);
                Stripe.Subscription sub = await _billing.Subscriptions.UpdateSubscriptionAsync(customer.Id, userSub.StripeId, plan);
                userSub = UpdateUserSubscriptionFromStripe(userSub, sub);
                userSub.Subscription = subscription;
                userSub.SubscriptionId = subscription.Id;
                UpdateUser(user);
            }
            else
                throw new Exception(BillingMessage.NoPaymentSource.ToString());

            return userSub;
        }
        public async Task<UserSubscription> CancelUserSubscription(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = GetCurrentUser();

            // Check for customer or throw.
            var customer = await LoadCustomerObject(user.StripeId, false);

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _billing.Subscriptions.CancelSubscriptionAsync(customer.Id, userSub.StripeId, true);
            userSub = UpdateUserSubscriptionFromStripe(userSub, sub);
            UpdateUser(user);
            return userSub;
        }
        public async Task<UserSubscription> RemoveUserSubscription(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = GetCurrentUser();

            // Check for customer or throw.
            var customer = await LoadCustomerObject(user.StripeId, false);

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _billing.Subscriptions.CancelSubscriptionAsync(customer.Id, userSub.StripeId, false);
            userSub = UpdateUserSubscriptionFromStripe(userSub, sub);
            UpdateUser(user);
            return userSub;
        }
        public async Task<UserSubscription> ReactivateUserSubscription(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = GetCurrentUser();

            // Check for customer or throw.
            var customer = await LoadCustomerObject(user.StripeId, false);

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _billing.Subscriptions.FindById(user.StripeId, userSub.StripeId);
            Stripe.SubscriptionUpdateOptions options = new Stripe.SubscriptionUpdateOptions();

            sub = await _billing.Subscriptions.UpdateSubscriptionAsync(customer.Id, sub.Id, sub.Plan);
            userSub = UpdateUserSubscriptionFromStripe(userSub, sub);
            UpdateUser(user);
            return userSub;
        }

        // [UserSubscription] Helpers
        private UserSubscription GetUserSubscription(ApplicationUser user, int userSubscriptionId)
        {
            UserSubscription subscription = user.Subscriptions.Where(us => us.Id == userSubscriptionId).FirstOrDefault();
            if (subscription == null)
                throw new Exception(BillingMessage.NoSubscription.ToString());
            return subscription;
        }
        private UserSubscription GetUserSubscriptionByStripeId(ApplicationUser user, string stripeId)
        {
            UserSubscription subscription = user.Subscriptions.Where(us => us.StripeId == stripeId).FirstOrDefault();
            if (subscription == null)
                throw new Exception(BillingMessage.NoSubscription.ToString());
            return subscription;
        }

        // WebHooks - ALL MUST RUN SYNCRONOUSLY
        public string ConfirmSubscriptionObject(Stripe.Subscription created, DateTime? eventTime)
        {
            StringWriter sw = new StringWriter();
            ApplicationUser user = GetUserByStripeId(created.CustomerId).Result;
            if (user == null)
                throw new Exception($"Could not locate user from Stripe id: {created.CustomerId}");

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, created.Id);
            if (userSub == null)
                throw new Exception($"Could not locate user's subscription from Stripe id: {created.Id}");

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.
            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
            {
                return sw.ToString();
            }

            userSub = UpdateUserSubscriptionFromStripe(userSub, created);
            userSub.Confirmed = true;
            _db.SaveChanges();

            return sw.ToString();
        }
        public string UpdateSubscriptionObject(Stripe.Subscription updated, DateTime? eventTime)
        {
            StringWriter sw = new StringWriter();
            ApplicationUser user = GetUserByStripeId(updated.CustomerId).Result;
            if (user == null)
                throw new Exception($"Could not locate user from Stripe id: {updated.CustomerId}");

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, updated.Id);
            if (userSub == null)
                throw new Exception($"Could not locate user's subscription from Stripe id: {updated.Id}");

            Models.Subscription plan = GetSubscriptionPlanByStripeId(updated.Plan.Id).Result;
            if (plan == null)
                throw new Exception($"Could not locate subscription plan object from Stripe id: {updated.Plan.Id}");

            if (userSub.SubscriptionId != plan.Id)
            {
                userSub.SubscriptionId = plan.Id;
            }

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.

            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
            {
                return sw.ToString();
            }

            userSub = UpdateUserSubscriptionFromStripe(userSub, updated);
            _db.SaveChanges();

            return sw.ToString();
        }
        public string RemoveUserSubscriptionObject(Stripe.Subscription deleted, DateTime? eventTime)
        {
            StringWriter sw = new StringWriter();
            ApplicationUser user = GetUserByStripeId(deleted.CustomerId).Result;
            if (user == null)
                throw new Exception($"Could not locate user from Stripe id: {deleted.CustomerId}");

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, deleted.Id);
            if (userSub == null)
                throw new Exception($"Could not locate user's subscription from Stripe id: {deleted.Id}");

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.
            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
            {
                return sw.ToString();
            }
            userSub = UpdateUserSubscriptionFromStripe(userSub, deleted);
            userSub.Deleted = true;
            userSub.DeletedAt = DateTime.Now;
            _db.SaveChanges();
            return sw.ToString();
        }
        private UserSubscription UpdateUserSubscriptionFromStripe(UserSubscription userSubscription, Stripe.Subscription stripeSubscription)
        {
            userSubscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;
            userSubscription.CanceledAt = stripeSubscription.CanceledAt;
            userSubscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
            userSubscription.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
            userSubscription.EndedAt = stripeSubscription.EndedAt;
            userSubscription.Quantity = stripeSubscription.Quantity ?? 0;
            userSubscription.Start = stripeSubscription.Start;
            userSubscription.Status = stripeSubscription.Status;
            userSubscription.TaxPercent = stripeSubscription.TaxPercent;
            userSubscription.TrialEnd = stripeSubscription.TrialEnd;
            userSubscription.TrialStart = stripeSubscription.TrialStart;
            userSubscription.Notes += DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + " Stripe.Event - Updated Subscription" + Environment.NewLine;
            userSubscription.LastUpdated = DateTime.Now;
            return userSubscription;
        }
        public UserSubscription FindUserSubscriptionByStripeId(string id)
        {
            UserSubscription userSub = _db.UserSubscriptions
                                .Include(s => s.User)
                                .Include(s => s.Subscription)
                                .FirstOrDefault(c => c.StripeId == id);
            return userSub;
        }


        public object GetStatistics()
        {
            var totalUsers = _db.Users.Count();
            var totalAdmins = _userManager.GetUsersInRoleAsync("Admin").Result.Count;
            var data = _db.Users.Select(c => new { date = c.CreatedOn.Date, month = c.CreatedOn.Month }).ToList();

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

        public object GetSubscriptionStatistics()
        {
            var total = _db.UserSubscriptions.Count();
            var trials = _db.UserSubscriptions.Where(c => c.Status == "trialing").Count();
            var active = _db.UserSubscriptions.Where(c => c.Status == "active").Count();

            var data = _db.UserSubscriptions.Where(c => c.Created.HasValue).Select(c => new { date = c.Created.Value.Date, month = c.Created.Value.Month }).ToList();

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

    }
}
