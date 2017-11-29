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
                User = GetUserById(userId)            
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
        public async Task<OperationResult> AddSubscriptionPlan(Subscription subscription)
        {
            try
            {
                // try adding to stripe
                var myPlan = new StripePlanCreateOptions()
                {
                    Id = subscription.StripeId,
                    Amount = subscription.Amount,                     // all amounts on Stripe are in cents, pence, etc
                    Currency = subscription.Currency,                 // "usd" only supported right now
                    Interval = subscription.Interval,                 // "month" or "year"
                    IntervalCount = subscription.IntervalCount,       // optional
                    Name = subscription.Name,
                    TrialPeriodDays = subscription.TrialPeriodDays   // amount of time that will lapse before the customer is billed
                };
                StripePlan response = await _billing.Stripe.PlanService.CreateAsync(myPlan);
                subscription.StripeId = response.Id;
                _db.Subscriptions.Add(subscription);
                var result = new OperationResult(_db.SaveChanges() == 1);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
        public async Task<OperationResult> DeleteSubscriptionPlan(int id)
        {
            try
            {
                Subscription subscription = _db.Subscriptions.Where(p => p.Id == id).FirstOrDefault();
                try
                {
                    await _billing.Stripe.PlanService.DeleteAsync(subscription.StripeId);
                }
                catch (StripeException)
                { }
                _db.SaveChanges();
                _db.Entry(subscription).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
        public async Task<Subscription> GetSubscriptionPlanById(int id)
        {
            Subscription subscription = await _db.Subscriptions
                                    .Include(s => s.Users)
                                    .Include(s => s.Features)
                                    .FirstOrDefaultAsync(c => c.Id == id);
            return subscription;
        }
        public async Task<Subscription> GetSubscriptionPlanByStripeId(string stripeId)
        {
            Subscription subscription = await _db.Subscriptions
                                .Include(s => s.Users)
                                .Include(s => s.Features)
                                .FirstOrDefaultAsync(c => c.StripeId == stripeId);

            return subscription;
        }
        private IQueryable<Subscription> GetSubscriptionPlans(string search = "", string sort = "", bool includeUsers = false)
        {
            IQueryable<Subscription> subscriptions = _db.Subscriptions.Include(s => s.Features);

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
        public async Task<List<Subscription>> GetSubscriptionPlansAsync()
        {
            return await GetSubscriptionPlans().ToListAsync();
        }
        public async Task<List<Subscription>> GetSubscriptionPlanLevels()
        {
            return await GetSubscriptionPlans().Where(s => s.Public && !s.Addon).OrderBy(s => s.Level).ToListAsync();
        }
        public async Task<List<Subscription>> GetSubscriptionPlanAddons()
        {
            return await GetSubscriptionPlans().Where(s => s.Public && s.Addon).ToListAsync();
        }
        public async Task<SubscriptionSearchModel> GetPagedSubscriptionPlans(SubscriptionSearchModel model)
        {
            var subs = await GetSubscriptionPlans(model.Search, model.Order, true).ToListAsync();
            model.List = subs;
            return model;
        }
        public async Task<OperationResult> UpdateSubscription(Subscription subscription)
        {
            try
            {
                var myPlan = new StripePlanUpdateOptions()
                {
                    Name = subscription.Name
                };
                StripePlan response = await _billing.Stripe.PlanService.UpdateAsync(subscription.StripeId, myPlan);
                _db.Update(subscription);
                await _db.SaveChangesAsync();
                return new OperationResult(true);
            }
            catch (DbUpdateException ex)
            {
                return new OperationResult(ex);
            }
        }

        // Stripe customer object
        public async Task<StripeCustomer> LoadCustomerObject(string stripeId, bool allowNullObject)
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
            Subscription subscription = await GetSubscriptionPlanById(planId);

            // Get the stripe subscription plan object.
            StripePlan plan = await _billing.SubscriptionPlans.FindByIdAsync(subscription.StripeId);

            // Check for customer or throw, but allow it to be null.
            StripeCustomer customer = await LoadCustomerObject(user.StripeId, true);

            // The object for the new user subscription to be recieved from stripe.
            StripeSubscription newSubscription = null;

            // if the user has provided a cardId to use, then let's try and use that!
            if (cardId.IsSet())
            {
                if (customer == null)
                    throw new Exception(BillingMessage.NoCustomerObject.ToString());

                // set the card as the default for the user, then subscribe the user.
                await _billing.Customers.SetDefaultCard(customer.Id, cardId);

                // check if the user is already on a subscription, if so, update that.
                var sub = (await _billing.Subscriptions.UserSubscriptionsAsync(user.StripeId)).FirstOrDefault(s => s.StripePlan.Id == plan.Id && s.Status != "canceled");
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
                StripeToken stripeTokenObj = _billing.Stripe.TokenService.Get(stripeToken);
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
                    var sub = (await _billing.Subscriptions.UserSubscriptionsAsync(user.StripeId)).FirstOrDefault(s => s.StripePlan.Id == plan.Id && s.Status != "canceled");
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

            Subscription subscription = await GetSubscriptionPlanById(planId);
            UserSubscription userSub = GetUserSubscription(user, subscriptionId);

            // Check for customer or throw.
            var customer = await LoadCustomerObject(user.StripeId, false);

            if (customer.DefaultSourceId.IsSet() || customer.Sources?.TotalCount > 0)
            {
                // there is a payment source - continue                  
                // Load the plan from stripe, then add to the user's subscription.
                StripePlan plan = await _billing.SubscriptionPlans.FindByIdAsync(subscription.StripeId);
                StripeSubscription sub = await _billing.Subscriptions.UpdateSubscriptionAsync(customer.Id, userSub.StripeId, plan);
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

            StripeSubscription sub = await _billing.Subscriptions.CancelSubscriptionAsync(customer.Id, userSub.StripeId, true);
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

            StripeSubscription sub = await _billing.Subscriptions.CancelSubscriptionAsync(customer.Id, userSub.StripeId, false);
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

            StripeSubscription sub = await _billing.Subscriptions.FindById(user.StripeId, userSub.StripeId);
            StripeSubscriptionUpdateOptions options = new StripeSubscriptionUpdateOptions();

            sub = await _billing.Subscriptions.UpdateSubscriptionAsync(customer.Id, sub.Id, sub.StripePlan);
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
        public string ConfirmSubscriptionObject(StripeSubscription created, DateTime? eventTime)
        {
            StringWriter sw = new StringWriter();
            try
            {
                sw.WriteLine("[System] Processing [AccountRepository.ConfirmSubscriptionObject].");
                ApplicationUser user = GetUserByStripeId(created.CustomerId).Result;
                UserSubscription userSub = GetUserSubscriptionByStripeId(user, created.Id);
                sw.WriteLine("[DB] [User] Name: " + user.FullName);
                sw.WriteLine("[DB] [User] Email: " + user.Email);
                sw.WriteLine("[DB] [User] Stripe: " + user.StripeId);
                sw.WriteLine("[DB] [UserSubscription] Stripe Id: " + userSub.StripeId);
                sw.WriteLine("[DB] [UserSubscription] Status: " + userSub.Status);
                sw.WriteLine("[DB] [UserSubscription] TrialEnd: " + userSub.TrialEnd);
                sw.WriteLine("[DB] [UserSubscription] TrialStart: " + userSub.TrialStart);
                sw.WriteLine("[DB] [UserSubscription] CurrentPeriodEnd: " + userSub.CurrentPeriodEnd);
                sw.WriteLine("[DB] [UserSubscription] CurrentPeriodStart: " + userSub.CurrentPeriodStart);
                sw.WriteLine("[DB] [UserSubscription] EndedAt: " + userSub.EndedAt);
                sw.WriteLine("[DB] [UserSubscription] DeletedAt: " + userSub.DeletedAt);
                sw.WriteLine("[DB] [UserSubscription] CanceledAt: " + userSub.CanceledAt);
                sw.WriteLine("[DB] [UserSubscription] (Plan) Id: " + userSub.SubscriptionId);
                sw.WriteLine("[DB] [UserSubscription] (Plan) Amount: " + userSub.Subscription.Amount);

                sw.WriteLine("[Stripe] [UserSubscription] Stripe Id: " + created.Id);
                sw.WriteLine("[Stripe] [UserSubscription] Status: " + created.Status);
                sw.WriteLine("[Stripe] [UserSubscription] TrialEnd: " + created.TrialEnd);
                sw.WriteLine("[Stripe] [UserSubscription] TrialStart: " + created.TrialStart);
                sw.WriteLine("[Stripe] [UserSubscription] CurrentPeriodEnd: " + created.CurrentPeriodEnd);
                sw.WriteLine("[Stripe] [UserSubscription] CurrentPeriodStart: " + created.CurrentPeriodStart);
                sw.WriteLine("[Stripe] [UserSubscription] EndedAt: " + created.EndedAt);
                sw.WriteLine("[Stripe] [UserSubscription] CanceledAt: " + created.CanceledAt);

                // Check the timestamp of the event, with the last update of the object
                // If this was updated last before the event, therefore the event is valid and should be applied.
                if (eventTime.HasValue && userSub.LastUpdated > eventTime)
                {
                    sw.WriteLine(string.Format("[System] Thread will be aborted, the database has been updated more recently than this event."));
                    sw.WriteLine(string.Format("[System] Timestamps... [eventTime] = {0} < [userSub.LastUpdated] = {1}", eventTime.Value.ToString(), userSub.LastUpdated.ToString()));
                    return sw.ToString();
                }

                sw.WriteLine("[System] [UpdateUserSubscriptionFromStripe(userSub, updated)]...");
                userSub = UpdateUserSubscriptionFromStripe(userSub, created);
                userSub.Confirmed = true;
                sw.WriteLine("[System] [UpdateUserSubscriptionFromStripe(userSub, updated)] complete.");
                sw.WriteLine("[System] Saving user subscription to database.");
                sw.WriteLine("[System] [UpdateUserSubscription(userSub)] complete.");
                _db.SaveChanges();
                sw.WriteLine("[System] Done.");

            }
            catch (Exception ex)
            {
                sw.WriteLine("[System] An error occurred while updating the subscription.");
                sw.WriteLine("[System] " + ex.Message);
                if (ex.InnerException != null)
                {
                    sw.WriteLine("[System] " + ex.InnerException.Message);
                }
            }
            return sw.ToString();
        }
        public string UpdateSubscriptionObject(StripeSubscription updated, DateTime? eventTime)
        {
            StringWriter sw = new StringWriter();
            try
            {
                sw.WriteLine("[System] Processing [AccountRepository.UpdateSubscriptionObject].");
                ApplicationUser user = GetUserByStripeId(updated.CustomerId).Result;
                UserSubscription userSub = GetUserSubscriptionByStripeId(user, updated.Id);
                sw.WriteLine("[DB] [User] Name: " + user.FullName);
                sw.WriteLine("[DB] [User] Email: " + user.Email);
                sw.WriteLine("[DB] [User] Stripe: " + user.StripeId);
                sw.WriteLine("[DB] [UserSubscription] Stripe Id: " + userSub.StripeId);
                sw.WriteLine("[DB] [UserSubscription] Status: " + userSub.Status);
                sw.WriteLine("[DB] [UserSubscription] TrialEnd: " + userSub.TrialEnd);
                sw.WriteLine("[DB] [UserSubscription] TrialStart: " + userSub.TrialStart);
                sw.WriteLine("[DB] [UserSubscription] CurrentPeriodEnd: " + userSub.CurrentPeriodEnd);
                sw.WriteLine("[DB] [UserSubscription] CurrentPeriodStart: " + userSub.CurrentPeriodStart);
                sw.WriteLine("[DB] [UserSubscription] EndedAt: " + userSub.EndedAt);
                sw.WriteLine("[DB] [UserSubscription] DeletedAt: " + userSub.DeletedAt);
                sw.WriteLine("[DB] [UserSubscription] CanceledAt: " + userSub.CanceledAt);
                sw.WriteLine("[DB] [UserSubscription] (Plan) Id: " + userSub.SubscriptionId);
                sw.WriteLine("[DB] [UserSubscription] (Plan) Amount: " + userSub.Subscription.Amount);

                sw.WriteLine("[Stripe] [UserSubscription] Stripe Id: " + updated.Id);
                sw.WriteLine("[Stripe] [UserSubscription] Status: " + updated.Status);
                sw.WriteLine("[Stripe] [UserSubscription] TrialEnd: " + updated.TrialEnd);
                sw.WriteLine("[Stripe] [UserSubscription] TrialStart: " + updated.TrialStart);
                sw.WriteLine("[Stripe] [UserSubscription] CurrentPeriodEnd: " + updated.CurrentPeriodEnd);
                sw.WriteLine("[Stripe] [UserSubscription] CurrentPeriodStart: " + updated.CurrentPeriodStart);
                sw.WriteLine("[Stripe] [UserSubscription] EndedAt: " + updated.EndedAt);
                sw.WriteLine("[Stripe] [UserSubscription] CanceledAt: " + updated.CanceledAt);

                Subscription plan = GetSubscriptionPlanByStripeId(updated.StripePlan.Id).Result;
                sw.WriteLine("[Stripe] [UserSubscription] (Plan) Id: " + plan.Id);
                sw.WriteLine("[Db] [UserSubscription] (Plan) Amount: " + updated.StripePlan.Amount);
                sw.WriteLine("[Stripe] [UserSubscription] (Plan-Stripe) Amount: " + updated.StripePlan.Amount);

                if (userSub.SubscriptionId != plan.Id)
                {
                    sw.WriteLine("[System] Plan has changed, [userSub.SubscriptionId != plan.Id], updating database records.");
                    userSub.SubscriptionId = plan.Id;
                    sw.WriteLine("[System] Done.");
                }
                // Check the timestamp of the event, with the last update of the object
                // If this was updated last before the event, therefore the event is valid and should be applied.

                if (eventTime.HasValue && userSub.LastUpdated > eventTime)
                {
                    sw.WriteLine(string.Format("[System] Thread will be aborted, the database has been updated more recently than this event."));
                    sw.WriteLine(string.Format("[System] Timestamps... [eventTime] = {0} < [userSub.LastUpdated] = {1}", eventTime.Value.ToString(), userSub.LastUpdated.ToString()));
                    return sw.ToString();
                }
                sw.WriteLine("[System] [UpdateUserSubscriptionFromStripe(userSub, updated)]...");
                userSub = UpdateUserSubscriptionFromStripe(userSub, updated);
                sw.WriteLine("[System] [UpdateUserSubscriptionFromStripe(userSub, updated)] complete.");
                sw.WriteLine("[System] Saving user subscription to database.");
                sw.WriteLine("[System] [UpdateUserSubscription(userSub)] complete.");
                _db.SaveChanges();
                sw.WriteLine("[System] Done.");
            }
            catch (Exception ex)
            {
                sw.WriteLine("[System] An error occurred while updating the subscription.");
                sw.WriteLine("[System] " + ex.Message);
                if (ex.InnerException != null)
                {
                    sw.WriteLine("[System] " + ex.InnerException.Message); 
                }
            }
            return sw.ToString();
        }
        public string RemoveUserSubscriptionObject(StripeSubscription deleted, DateTime? eventTime)
        {
            StringWriter sw = new StringWriter();
            try
            {
                sw.WriteLine("[System] Processing [AccountRepository.RemoveUserSubscriptionObject].");
                ApplicationUser user = GetUserByStripeId(deleted.CustomerId).Result;
                UserSubscription userSub = GetUserSubscriptionByStripeId(user, deleted.Id);
                sw.WriteLine("User: " + user.FullName);
                sw.WriteLine("User Email: " + user.Email);
                sw.WriteLine("User Stripe: " + user.StripeId);
                sw.WriteLine("[DB] [UserSubscription] Stripe Id: " + userSub.StripeId);
                sw.WriteLine("[DB] [UserSubscription] Status: " + userSub.Status);
                sw.WriteLine("[DB] [UserSubscription] TrialEnd: " + userSub.TrialEnd);
                sw.WriteLine("[DB] [UserSubscription] TrialStart: " + userSub.TrialStart);
                sw.WriteLine("[DB] [UserSubscription] CurrentPeriodEnd: " + userSub.CurrentPeriodEnd);
                sw.WriteLine("[DB] [UserSubscription] CurrentPeriodStart: " + userSub.CurrentPeriodStart);
                sw.WriteLine("[DB] [UserSubscription] EndedAt: " + userSub.EndedAt);
                sw.WriteLine("[DB] [UserSubscription] DeletedAt: " + userSub.DeletedAt);
                sw.WriteLine("[DB] [UserSubscription] CanceledAt: " + userSub.CanceledAt);
                sw.WriteLine("[DB] [UserSubscription] (Plan) Id: " + userSub.SubscriptionId);
                sw.WriteLine("[DB] [UserSubscription] (Plan) Amount: " + userSub.Subscription.Amount);

                sw.WriteLine("[Stripe] [UserSubscription] Stripe Id: " + deleted.Id);
                sw.WriteLine("[Stripe] [UserSubscription] Status: " + deleted.Status);
                sw.WriteLine("[Stripe] [UserSubscription] TrialEnd: " + deleted.TrialEnd);
                sw.WriteLine("[Stripe] [UserSubscription] TrialStart: " + deleted.TrialStart);
                sw.WriteLine("[Stripe] [UserSubscription] CurrentPeriodEnd: " + deleted.CurrentPeriodEnd);
                sw.WriteLine("[Stripe] [UserSubscription] CurrentPeriodStart: " + deleted.CurrentPeriodStart);
                sw.WriteLine("[Stripe] [UserSubscription] EndedAt: " + deleted.EndedAt);
                sw.WriteLine("[Stripe] [UserSubscription] CanceledAt: " + deleted.CanceledAt);

                // Check the timestamp of the event, with the last update of the object
                // If this was updated last before the event, therefore the event is valid and should be applied.
                if (eventTime.HasValue && userSub.LastUpdated > eventTime)
                {
                    sw.WriteLine(string.Format("[System] Thread will be aborted, the database has been updated more recently than this event."));
                    sw.WriteLine(string.Format("[System] Timestamps... [eventTime] = {0} < [userSub.LastUpdated] = {1}", eventTime.Value.ToString(), userSub.LastUpdated.ToString()));
                    return sw.ToString();
                }
                sw.WriteLine("[System] [UpdateUserSubscriptionFromStripe(userSub, updated)]...");
                userSub = UpdateUserSubscriptionFromStripe(userSub, deleted);
                userSub.Deleted = true;
                userSub.DeletedAt = DateTime.Now;
                sw.WriteLine("[System] [UpdateUserSubscriptionFromStripe(userSub, updated)] complete.");
                sw.WriteLine("[System] Saving user subscription to database.");
                sw.WriteLine("[System] [UpdateUserSubscription(userSub)] complete.");
                _db.SaveChanges();
                sw.WriteLine("[System] Done.");
            }
            catch (Exception ex)
            {
                sw.WriteLine("[System] An error occurred while updating the subscription.");
                sw.WriteLine("[System] " + ex.Message);
                if (ex.InnerException != null)
                {
                    sw.WriteLine("[System] " + ex.InnerException.Message);
                }
            }
            return sw.ToString();
        }
        private UserSubscription UpdateUserSubscriptionFromStripe(UserSubscription userSubscription, StripeSubscription stripeSubscription)
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
            userSubscription.Notes += DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + " StripeEvent - Updated Subscription" + Environment.NewLine;
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

    }
}
