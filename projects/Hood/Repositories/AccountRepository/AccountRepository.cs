using Hood.Core;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStripeService _stripe;

        public AccountRepository(
            HoodDbContext db,
            IStripeService stripe,
            IHttpContextAccessor context,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _contextAccessor = context;
            _userManager = userManager;
            _stripe = stripe;
        }

        #region User Get/Update/Delete
        private IQueryable<ApplicationUser> UserQuery
        {
            get
            {
                Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ApplicationUser, Models.Subscription> query = _db.Users
                    .Include(u => u.Addresses)
                    .Include(u => u.AccessCodes)
                    .Include(u => u.Subscriptions).ThenInclude(u => u.Subscription);
                return query;
            }
        }
        public async Task<ApplicationUser> GetUserByIdAsync(string id, bool track = true)
        {
            if (!id.IsSet())
            {
                return null;
            }

            IQueryable<ApplicationUser> query = UserQuery;
            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(u => u.Id == id);
        }
        public async Task<UserProfile> GetUserProfileByIdAsync(string id)
        {
            if (!id.IsSet())
            {
                return null;
            }
            return await _db.UserProfiles.SingleOrDefaultAsync(u => u.Id == id);
        }
        public async Task<ApplicationUser> GetUserByEmailAsync(string email, bool track = true)
        {
            if (!email.IsSet())
            {
                return null;
            }

            IQueryable<ApplicationUser> query = UserQuery;
            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(u => u.Email == email);
        }
        public async Task<ApplicationUser> GetUserByStripeIdAsync(string stripeId, bool track = true)
        {
            if (!stripeId.IsSet())
            {
                return null;
            }

            IQueryable<ApplicationUser> query = UserQuery;
            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(u => u.StripeId == stripeId);
        }
        public async Task<ApplicationUser> GetCurrentUserAsync(bool track = true)
        {
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return await GetUserByIdAsync(_userManager.GetUserId(_contextAccessor.HttpContext.User), track);
            }
            else
            {
                return null;
            }
        }
        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _db.Update(user);
            await _db.SaveChangesAsync();
        }
        public async Task DeleteUserAsync(string userId, System.Security.Claims.ClaimsPrincipal adminUser)
        {
            var siteOwner = Engine.Settings.SiteOwner;

            if (userId == siteOwner.Id)
                throw new Exception("You cannot delete the site owner, you must assign a new site owner (from the site owner account) before you can delete this account.");

            var user = await _db.Users
                .Include(u => u.Content)
                .Include(u => u.Properties)
                .Include(u => u.Forums)
                .Include(u => u.Topics)
                .Include(u => u.Posts)
                .Include(u => u.Posts)
                .SingleOrDefaultAsync(u => u.Id == userId);


            if (adminUser.IsInRole("SuperAdmin") || adminUser.IsInRole("Admin") || adminUser.GetUserId() == user.Id)
            {
                IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);
                foreach (UserLoginInfo li in logins)
                {
                    await _userManager.RemoveLoginAsync(user, li.LoginProvider, li.ProviderKey);
                }

                IList<string> roles = await _userManager.GetRolesAsync(user);
                foreach (string role in roles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

                IList<System.Security.Claims.Claim> claims = await _userManager.GetClaimsAsync(user);
                foreach (System.Security.Claims.Claim claim in claims)
                {
                    await _userManager.RemoveClaimAsync(user, claim);
                }

                // Delete any content or access codes attached to the user
                user.Topics.ForEach(t => _db.Entry(t).State = EntityState.Deleted);
                user.Posts.ForEach(p => _db.Entry(p).State = EntityState.Deleted);

                // Set any site content as owned by the site owner, instead of the user.
                user.Content.ForEach(c => c.AuthorId = siteOwner.Id);
                user.Properties.ForEach(p => p.AgentId = siteOwner.Id);
                user.Forums.ForEach(f => f.AuthorId = siteOwner.Id);

                var userSubs = await GetUserProfileByIdAsync(user.Id);
                if (userSubs.ActiveSubscriptions != null)
                {
                    if (userSubs.ActiveSubscriptions.Count > 0)
                    {
                        throw new Exception("You cannot delete a user with active subscriptions, cancel them or delete them before deleting the user.");
                    }
                }

                if (userSubs.StripeId.IsSet())
                    await _stripe.DeleteCustomerAsync(userSubs.StripeId);

                await _db.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
            }
            else
            {
                throw new Exception("You do not have permission to delete this user.");
            }
        }
        public async Task<MediaDirectory> GetDirectoryAsync(string userId)
        {
            MediaDirectory directory = await _db.MediaDirectories.SingleOrDefaultAsync(md => md.OwnerId == userId && md.Type == DirectoryType.User);
            if (directory == null)
            {
                Task<MediaDirectory> userDirectory = _db.MediaDirectories.SingleOrDefaultAsync(md => md.Slug == MediaManager.UserDirectorySlug && md.Type == DirectoryType.System);
                ApplicationUser user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("No user found to add/get directory for.");
                }

                directory = new MediaDirectory()
                {
                    OwnerId = userId,
                    Type = DirectoryType.User,
                    ParentId = userDirectory.Id,
                    DisplayName = user.UserName,
                    Slug = user.Id
                };
                _db.Add(directory);
                await _db.SaveChangesAsync();
            }
            return directory;
        }
        #endregion

        #region Profiles 
        public async Task<UserListModel> GetUserProfilesAsync(UserListModel model)
        {
            IQueryable<UserProfile> query = _db.UserProfiles.AsQueryable();

            if (model.Role.IsSet())
            {
                query = query.Where(q => q.RoleIds.Contains(model.Role));
            }

            if (model.RoleIds != null && model.RoleIds.Count > 0)
            {
                query = query.Where(q => q.RoleIds != null && model.RoleIds.Any(m => q.RoleIds.Contains(m)));
            }

            if (model.Subscription.IsSet())
            {
                query = query.Where(q => q.ActiveSubscriptionIds.Contains(model.Subscription));
            }

            if (model.SubscriptionIds != null && model.SubscriptionIds.Count > 0)
            {
                query = query.Where(q => q.ActiveSubscriptionIds != null && model.SubscriptionIds.Any(m => q.ActiveSubscriptionIds.Contains(m)));
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
            UserProfile profile = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id);
            return profile;
        }
        public async Task<List<UserAccessCode>> GetAccessCodesAsync(string id)
        {
            return await _db.AccessCodes.Where(u => u.UserId == id).ToListAsync();
        }
        public async Task UpdateProfileAsync(UserProfile user)
        {
            ApplicationUser userToUpdate = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            foreach (PropertyInfo property in typeof(IUserProfile).GetProperties())
            {
                property.SetValue(userToUpdate, property.GetValue(user));
            }

            foreach (PropertyInfo property in typeof(IName).GetProperties())
            {
                property.SetValue(userToUpdate, property.GetValue(user));
            }

            foreach (PropertyInfo property in typeof(IAvatar).GetProperties())
            {
                property.SetValue(userToUpdate, property.GetValue(user));
            }

            foreach (PropertyInfo property in typeof(IJsonMetadata).GetProperties())
            {
                property.SetValue(userToUpdate, property.GetValue(user));
            }

            _db.Update(userToUpdate);
            _db.SaveChanges();
        }
        #endregion

        #region Roles
        public async Task<IList<IdentityRole>> GetAllRolesAsync()
        {
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
            Task<Models.Address> address = GetAddressByIdAsync(id);
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
            ApplicationUser user = await GetUserByIdAsync(userId);
            Models.Address add = user.Addresses.SingleOrDefault(a => a.Id == id);
            if (add != null)
            {
                user.BillingAddress = add.CloneTo<Models.Address>();
            }

            await UpdateUserAsync(user);
        }
        public async Task SetDeliveryAddressAsync(string userId, int id)
        {
            ApplicationUser user = await GetUserByIdAsync(userId);
            Models.Address add = user.Addresses.SingleOrDefault(a => a.Id == id);
            if (add != null)
            {
                user.DeliveryAddress = add.CloneTo<Models.Address>();
            }

            await UpdateUserAsync(user);
        }
        #endregion

        #region Subscription Products
        public async Task<SubscriptionProductListModel> GetSubscriptionProductsAsync(SubscriptionProductListModel model = null)
        {
            IQueryable<SubscriptionProduct> query = _db.SubscriptionProducts.Include(g => g.Subscriptions).AsQueryable();

            if (model == null)
            {
                model = new SubscriptionProductListModel();
            }

            // search the collection
            if (!string.IsNullOrEmpty(model.Search))
            {

                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(n => searchTerms.Any(s => n.DisplayName != null && n.DisplayName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Body != null && n.Body.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.FeaturesJson != null && n.FeaturesJson.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }


            // sort the collection and then output it.
            if (!string.IsNullOrEmpty(model.Order))
            {
                switch (model.Order)
                {
                    case "Title":
                        query = query.OrderBy(n => n.DisplayName);
                        break;

                    case "TitleDesc":
                        query = query.OrderByDescending(n => n.DisplayName);
                        break;

                    default:
                        query = query.OrderBy(n => n.DisplayName).ThenBy(n => n.Id);
                        break;
                }
            }

            await model.ReloadAsync(query);

            return model;
        }
        public async Task<StripeProductListModel> GetStripeProductsAsync(StripeProductListModel model = null)
        {
            SubscriptionProductListModel localPlans = await GetSubscriptionProductsAsync();
            IEnumerable<Product> stripePlans = await _stripe.GetAllProductsAsync();

            List<ConnectedStripeProduct> connectedPlans = new List<ConnectedStripeProduct>();

            stripePlans.ForEach(sp =>
            {
                ConnectedStripeProduct csp = new ConnectedStripeProduct(sp);
                SubscriptionProduct link = localPlans.List.SingleOrDefault(lp => lp.StripeId == sp.Id);
                if (link != null)
                {
                    csp.SubscriptionProduct = link;
                    csp.SubscriptionProductId = link.Id;
                }
                connectedPlans.Add(csp);
            });

            if (model.Linked)
            {
                connectedPlans = connectedPlans.Where(sp => sp.SubscriptionProduct != null).ToList();
            }

            if (model.Search.IsSet())
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                connectedPlans = connectedPlans.Where(n => searchTerms.Any(s => n.Name != null && n.Name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Id != null && n.Id.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)).ToList();
            }


            if (model.Order.IsSet())
            {
                switch (model.Order)
                {
                    case "Name":
                        connectedPlans = connectedPlans.OrderBy(n => n.SubscriptionProduct?.DisplayName).ToList();
                        break;
                    case "Created":
                        connectedPlans = connectedPlans.OrderBy(n => n.SubscriptionProduct?.Created).ToList();
                        break;

                    case "NameDesc":
                        connectedPlans = connectedPlans.OrderByDescending(n => n.SubscriptionProduct?.DisplayName).ToList();
                        break;
                    case "CreatedDesc":
                        connectedPlans = connectedPlans.OrderByDescending(n => n.SubscriptionProduct?.Created).ToList();
                        break;

                    default:
                        connectedPlans = connectedPlans.OrderBy(n => n.Id).ToList();
                        break;
                }
            }

            model.Reload(connectedPlans, model.PageIndex, model.PageSize);

            return model;
        }
        public async Task<SubscriptionProduct> GetSubscriptionProductByIdAsync(int id)
        {
            SubscriptionProduct subscriptionProduct = await _db.SubscriptionProducts
                                    .Include(s => s.Subscriptions)
                                    .SingleOrDefaultAsync(c => c.Id == id);
            return subscriptionProduct;
        }
        public async Task<SubscriptionProduct> GetSubscriptionProductByStripeIdAsync(string stripeId)
        {
            SubscriptionProduct subscriptionProduct = await _db.SubscriptionProducts
                                    .Include(s => s.Subscriptions)
                                    .FirstOrDefaultAsync(c => c.StripeId == stripeId);
            return subscriptionProduct;
        }
        public async Task<SubscriptionProduct> CreateSubscriptionProductAsync(string name, string stripeId)
        {
            var subscriptionProduct = new SubscriptionProduct()
            {
                DisplayName = name,
                Slug = name.ToSeoUrl(),
                StripeId = stripeId,
                CreatedBy = Engine.Account.Id,
                Created = DateTime.Now,
                LastEditedBy = Engine.Account.UserName,
                LastEditedOn = DateTime.Now
            };
            if (subscriptionProduct.StripeId.IsSet())
            {
                ProductUpdateOptions productUpdate = new ProductUpdateOptions()
                {
                    Name = subscriptionProduct.DisplayName
                };
                await _stripe.UpdateProductAsync(subscriptionProduct.StripeId, productUpdate);
            }
            else
            {
                Product response = await _stripe.CreateProductAsync(subscriptionProduct.DisplayName);
                subscriptionProduct.StripeId = response.Id;
            }
            _db.SubscriptionProducts.Add(subscriptionProduct);
            await _db.SaveChangesAsync();
            return subscriptionProduct;
        }
        public async Task<SubscriptionProduct> UpdateSubscriptionProductAsync(SubscriptionProduct subscriptionProduct)
        {
            if (subscriptionProduct.StripeId.IsSet())
            {
                ProductUpdateOptions productUpdate = new ProductUpdateOptions()
                {
                    Name = subscriptionProduct.DisplayName
                };
                await _stripe.UpdateProductAsync(subscriptionProduct.StripeId, productUpdate);
            }
            else
            {
                Product response = await _stripe.CreateProductAsync(subscriptionProduct.DisplayName);
                subscriptionProduct.StripeId = response.Id;
            }
            _db.Update(subscriptionProduct);
            await _db.SaveChangesAsync();
            return subscriptionProduct;
        }
        public async Task DeleteSubscriptionProductAsync(int id)
        {
            SubscriptionProduct subscriptionProduct = _db.SubscriptionProducts
                .Include(s => s.Subscriptions)
                .Where(p => p.Id == id)
                .FirstOrDefault();

            if (subscriptionProduct.Subscriptions != null && subscriptionProduct.Subscriptions.Count() > 0)
                throw new Exception("You cannot remove a subscription product group which contains plans. Move them to a new product group or delete them before you remove the product group.");

            var stripePlan = await _stripe.GetProductByIdAsync(subscriptionProduct.StripeId);
            if (stripePlan != null)
            {
                var subcount = await _stripe.PlanService.ListAsync(new PlanListOptions() { Limit = 1, ProductId = subscriptionProduct.StripeId });
                if (subcount != null && subcount.Count() > 0)
                    throw new Exception("You cannot remove a subscription product group which contains plans (on Stripe). Move them to a new product group or delete them before you remove the product group.");

                await _stripe.DeleteProductAsync(subscriptionProduct.StripeId);
            }

            _db.SaveChanges();
            _db.Entry(subscriptionProduct).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            return;
        }
        public async Task<SubscriptionProduct> SyncSubscriptionProductAsync(int? id, string stripeId)
        {
            SubscriptionProduct product = null;
            if (id.HasValue)
            {
                product = await GetSubscriptionProductByIdAsync(id.Value);
            }

            Stripe.Product stripeProduct = null;
            if (stripeId.IsSet())
            {
                stripeProduct = await _stripe.GetProductByIdAsync(stripeId);

                if (product == null)
                    product = await GetSubscriptionProductByStripeIdAsync(stripeId);
            }

            if (product == null && stripeProduct == null)
            {
                throw new Exception("Could not sync, neither a local or a stripe object could be loaded.");
            }
            else if (product == null && stripeProduct != null)
            {
                await CreateSubscriptionProductAsync(stripeProduct.Name, stripeProduct.Id);
            }
            else if (product != null && stripeProduct == null)
            {
                stripeProduct = await _stripe.CreateProductAsync(product.DisplayName);
                product.StripeId = stripeProduct.Id;
                await UpdateSubscriptionProductAsync(product);
            }
            else
            {
                // Update from stripe.         
                product.DisplayName = stripeProduct.Name;
                product.StripeId = stripeProduct.Id;
                product.LastEditedBy = Engine.Account.UserName;
                product.LastEditedOn = DateTime.Now;
                await UpdateSubscriptionProductAsync(product);
            }

            // now go through all local plans associated
            product = await GetSubscriptionProductByIdAsync(id.Value);
            if (product.Subscriptions.Count > 0)
            {
                foreach (var s in product.Subscriptions)
                {
                    await SyncSubscriptionPlanAsync(s.Id, s.StripeId);
                }
            }

            var plans = await _stripe.GetAllPlansAsync(new PlanListOptions() { ProductId = product.StripeId });
            foreach (var p in plans)
            {
                await SyncSubscriptionPlanAsync(null, p.Id);
            }

            return product;
        }
        #endregion

        #region Subscription Plans
        public async Task<SubscriptionPlanListModel> GetSubscriptionPlansAsync(SubscriptionPlanListModel model = null)
        {
            IQueryable<SubscriptionPlan> query = _db.SubscriptionPlans.Include(s => s.SubscriptionProduct);

            if (model == null)
            {
                model = new SubscriptionPlanListModel() { PageSize = int.MaxValue };
            }

            if (model.ProductId.HasValue)
            {
                query = query.Where(u => u.SubscriptionProductId == model.ProductId);
            }

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
                    case "Name":
                        query = query.OrderBy(n => n.Name);
                        break;
                    case "Created":
                        query = query.OrderBy(n => n.Created);
                        break;
                    case "Amount":
                        query = query.OrderBy(n => n.Amount);
                        break;
                    case "ActiveCount":
                        query = query.OrderBy(n => n.ActiveCount);
                        break;
                    case "TrialCount":
                        query = query.OrderBy(n => n.TrialCount);
                        break;

                    case "NameDesc":
                        query = query.OrderByDescending(n => n.Name);
                        break;
                    case "CreatedDesc":
                        query = query.OrderByDescending(n => n.Created);
                        break;
                    case "AmountDesc":
                        query = query.OrderByDescending(n => n.Amount);
                        break;
                    case "ActiveCountDesc":
                        query = query.OrderByDescending(n => n.ActiveCount);
                        break;
                    case "TrialCountDesc":
                        query = query.OrderByDescending(n => n.TrialCount);
                        break;

                    default:
                        query = query.OrderByDescending(n => n.Created).ThenBy(n => n.Id);
                        break;
                }
            }

            await model.ReloadAsync(query);

            return model;
        }
        public async Task<StripePlanListModel> GetStripeSubscriptionPlansAsync(StripePlanListModel model = null)
        {
            SubscriptionPlanListModel localPlans = await GetSubscriptionPlansAsync();
            IEnumerable<Plan> stripePlans = await _stripe.GetAllPlansAsync();

            List<ConnectedStripePlan> connectedPlans = new List<ConnectedStripePlan>();

            stripePlans.ForEach(sp =>
            {
                ConnectedStripePlan csp = new ConnectedStripePlan(sp);
                SubscriptionPlan link = localPlans.List.SingleOrDefault(lp => lp.StripeId == sp.Id);
                if (link != null)
                {
                    csp.SubscriptionPlan = link;
                    csp.SubscriptionPlanId = link.Id;
                }
                connectedPlans.Add(csp);
            });

            if (model.Linked)
            {
                connectedPlans = connectedPlans.Where(sp => sp.SubscriptionPlan != null).ToList();
            }

            if (model.Search.IsSet())
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                connectedPlans = connectedPlans.Where(n => searchTerms.Any(s => n.Nickname != null && n.Nickname.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Id != null && n.Id.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)).ToList();
            }


            if (model.Order.IsSet())
            {
                switch (model.Order)
                {
                    case "Name":
                        connectedPlans = connectedPlans.OrderBy(n => n.SubscriptionPlan?.Name).ToList();
                        break;
                    case "Created":
                        connectedPlans = connectedPlans.OrderBy(n => n.SubscriptionPlan?.Created).ToList();
                        break;
                    case "Amount":
                        connectedPlans = connectedPlans.OrderBy(n => n.SubscriptionPlan?.Amount).ToList();
                        break;
                    case "ActiveCount":
                        connectedPlans = connectedPlans.OrderBy(n => n.SubscriptionPlan?.ActiveCount).ToList();
                        break;
                    case "TrialCount":
                        connectedPlans = connectedPlans.OrderBy(n => n.SubscriptionPlan?.TrialCount).ToList();
                        break;

                    case "NameDesc":
                        connectedPlans = connectedPlans.OrderByDescending(n => n.SubscriptionPlan?.Name).ToList();
                        break;
                    case "CreatedDesc":
                        connectedPlans = connectedPlans.OrderByDescending(n => n.SubscriptionPlan?.Created).ToList();
                        break;
                    case "AmountDesc":
                        connectedPlans = connectedPlans.OrderByDescending(n => n.SubscriptionPlan?.Amount).ToList();
                        break;
                    case "ActiveCountDesc":
                        connectedPlans = connectedPlans.OrderByDescending(n => n.SubscriptionPlan?.ActiveCount).ToList();
                        break;
                    case "TrialCountDesc":
                        connectedPlans = connectedPlans.OrderByDescending(n => n.SubscriptionPlan?.TrialCount).ToList();
                        break;

                    default:
                        connectedPlans = connectedPlans.OrderBy(n => n.Id).ToList();
                        break;
                }
            }

            model.Reload(connectedPlans, model.PageIndex, model.PageSize);

            return model;
        }
        public async Task<Models.Subscription> GetSubscriptionPlanByIdAsync(int id)
        {
            Models.Subscription subscription = await _db.Subscriptions
                                    .Include(s => s.Users)
                                    .Include(s => s.SubscriptionProduct)
                                    .FirstOrDefaultAsync(c => c.Id == id);
            return subscription;
        }
        public async Task<Models.Subscription> GetSubscriptionPlanByStripeIdAsync(string stripeId)
        {
            Models.Subscription subscription = await _db.Subscriptions
                                .Include(s => s.Users)
                                .Include(s => s.SubscriptionProduct)
                                .FirstOrDefaultAsync(c => c.StripeId == stripeId);

            return subscription;
        }
        public async Task<Models.Subscription> CreateSubscriptionPlanAsync(Models.Subscription subscription)
        {
            _db.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync();
            SubscriptionProduct group = await SyncProductForSubscriptionPlanAsync(subscription);
            if (!subscription.StripeId.IsSet())
            {
                Plan response = await _stripe.CreatePlanAsync(subscription.Name, subscription.Amount, subscription.Currency, subscription.Interval, subscription.IntervalCount, subscription.TrialPeriodDays, group.StripeId);
                subscription.StripeId = response.Id;
            }
            return subscription;
        }
        public async Task<Models.Subscription> UpdateSubscriptionPlanAsync(Models.Subscription subscription)
        {
            SubscriptionProduct group = await SyncProductForSubscriptionPlanAsync(subscription);
            await UpdateStripeSubscriptionPlan(subscription, group);
            _db.Entry(subscription).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return subscription;
        }
        public async Task DeleteSubscriptionPlanAsync(int id)
        {
            Models.Subscription subscription = _db.Subscriptions.Where(p => p.Id == id).FirstOrDefault();

            var stripePlan = await _stripe.GetPlanByIdAsync(subscription.StripeId);
            if (stripePlan != null)
            {
                var subcount = await _stripe.SubscriptionService.ListAsync(new SubscriptionListOptions() { Limit = 1, PlanId = subscription.StripeId, Status = SubscriptionStatuses.Active });
                if (subcount != null && subcount.Count() > 0)
                    throw new Exception("You cannot remove a subscription with active subscribers. Move them to a new subscription on Stripe or cancel them before you remove the plan.");
                subcount = await _stripe.SubscriptionService.ListAsync(new SubscriptionListOptions() { Limit = 1, PlanId = subscription.StripeId, Status = SubscriptionStatuses.Trialing });
                if (subcount != null && subcount.Count() > 0)
                    throw new Exception("You cannot remove a subscription with trial subscribers. Move them to a new subscription on Stripe or cancel them before you remove the plan.");

                await _stripe.DeletePlanAsync(subscription.StripeId);
            }

            _db.SaveChanges();
            _db.Entry(subscription).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            return;
        }
        public async Task<Models.Subscription> SyncSubscriptionPlanAsync(int? id, string stripeId)
        {
            Models.Subscription subscriptionPlan = null;
            if (id.HasValue)
            {
                subscriptionPlan = await GetSubscriptionPlanByIdAsync(id.Value);
            }

            Stripe.Plan stripePlan = null;
            if (stripeId.IsSet())
            {
                stripePlan = await _stripe.GetPlanByIdAsync(stripeId);
            }

            if (subscriptionPlan == null && stripePlan == null)
            {
                throw new Exception("Could not sync, neither a local or a stripe object could be loaded.");
            }
            else if (subscriptionPlan == null && stripePlan != null)
            {
                // Check it doesnt already exist, perhaps in the wrong place
                subscriptionPlan = await GetSubscriptionPlanByStripeIdAsync(stripePlan.Id);
                if (subscriptionPlan == null)
                {
                    subscriptionPlan = new Models.Subscription()
                    {
                        Name = stripePlan.Nickname,
                        StripeId = stripePlan.Id,
                        CreatedBy = Engine.Account.Id,
                        Created = DateTime.Now,
                        LastEditedBy = Engine.Account.UserName,
                        LastEditedOn = DateTime.Now,
                        Addon = false,
                        Public = stripePlan.Active,
                        Amount = (int)stripePlan.Amount,
                        Currency = stripePlan.Currency,
                        Interval = stripePlan.Interval,
                        IntervalCount = (int)stripePlan.IntervalCount,
                        NumberAllowed = 1,
                    };
                    if (stripePlan.TrialPeriodDays.HasValue)
                    {
                        subscriptionPlan.TrialPeriodDays = (int)stripePlan.TrialPeriodDays.Value;
                    }
                    else
                    {
                        subscriptionPlan.TrialPeriodDays = null;
                    }
                    subscriptionPlan = await CreateSubscriptionPlanAsync(subscriptionPlan);
                }
                else
                {
                    subscriptionPlan = await UpdatePlanFromStripe(subscriptionPlan, stripePlan);
                }
            }
            else if (subscriptionPlan != null && stripePlan == null)
            {
                subscriptionPlan = await UpdateSubscriptionPlanAsync(subscriptionPlan);
            }
            else
            {
                subscriptionPlan = await UpdatePlanFromStripe(subscriptionPlan, stripePlan);
            }
            await SyncProductForSubscriptionPlanAsync(subscriptionPlan);
            return subscriptionPlan;
        }

        private async Task<SubscriptionProduct> SyncProductForSubscriptionPlanAsync(Models.Subscription subscriptionPlan)
        {
            SubscriptionProduct product = null;
            if (subscriptionPlan.SubscriptionProductId.HasValue)
            {
                product = await _db.SubscriptionProducts.SingleOrDefaultAsync(c => c.Id == subscriptionPlan.SubscriptionProductId.Value);
            }
            if (subscriptionPlan.StripeId.IsSet())
            {
                Plan stripePlan = await _stripe.GetPlanByIdAsync(subscriptionPlan.StripeId);
                if (product != null)
                {
                    if (product.StripeId != stripePlan.ProductId)
                    {
                        // its in the wrong group - try to load the correct one, if none can be loaded, then this will be created
                        product = await _db.SubscriptionProducts.SingleOrDefaultAsync(c => c.StripeId == stripePlan.ProductId);
                    }
                }
                else
                {
                    if (stripePlan != null)
                    {
                        product = await _db.SubscriptionProducts.SingleOrDefaultAsync(c => c.StripeId == stripePlan.ProductId);
                    }
                }
            }
            if (product != null)
            {
                if (product.StripeId == null)
                    await _stripe.CreateProductAsync(product.DisplayName);

                Product stripeProduct = await _stripe.GetProductByIdAsync(product.StripeId);
                if (stripeProduct == null)
                    await _stripe.CreateProductAsync(product.DisplayName);

                if (stripeProduct != null)
                    product.StripeId = stripeProduct.Id;

                subscriptionPlan.SubscriptionProductId = product.Id;
            }
            else
            {
                product = new SubscriptionProduct()
                {
                    DisplayName = subscriptionPlan.Name,
                    Slug = subscriptionPlan.Name.ToSeoUrl()
                };
                _db.SubscriptionProducts.Add(product);
                await _db.SaveChangesAsync();
                Product stripeProduct = await _stripe.CreateProductAsync(subscriptionPlan.Name);
                subscriptionPlan.SubscriptionProductId = product.Id;
                product.StripeId = stripeProduct.Id;
            }

            if (!subscriptionPlan.StripeId.IsSet())
            {
                Plan response = await _stripe.CreatePlanAsync(
                    subscriptionPlan.Name,
                    subscriptionPlan.Amount,
                    subscriptionPlan.Currency,
                    subscriptionPlan.Interval,
                    subscriptionPlan.IntervalCount,
                    subscriptionPlan.TrialPeriodDays,
                    product.StripeId);
                subscriptionPlan.StripeId = response.Id;
            }

            await _db.SaveChangesAsync();
            return product;
        }
        private async Task<Models.Subscription> UpdatePlanFromStripe(Models.Subscription subscriptionPlan, Plan stripePlan)
        {
            // Update from stripe.      
            subscriptionPlan.Name = stripePlan.Nickname;
            subscriptionPlan.StripeId = stripePlan.Id;
            subscriptionPlan.LastEditedBy = Engine.Account.UserName;
            subscriptionPlan.LastEditedOn = DateTime.Now;
            subscriptionPlan.Addon = false;
            subscriptionPlan.Public = stripePlan.Active;
            subscriptionPlan.Amount = (int)stripePlan.Amount;
            subscriptionPlan.Currency = stripePlan.Currency;
            subscriptionPlan.Interval = stripePlan.Interval;
            subscriptionPlan.IntervalCount = (int)stripePlan.IntervalCount;
            if (stripePlan.TrialPeriodDays.HasValue)
            {
                subscriptionPlan.TrialPeriodDays = (int)stripePlan.TrialPeriodDays.Value;
            }
            else
            {
                subscriptionPlan.TrialPeriodDays = null;
            }
            subscriptionPlan = await UpdateSubscriptionPlanAsync(subscriptionPlan);
            return subscriptionPlan;
        }
        private async Task UpdateStripeSubscriptionPlan(Models.Subscription subscription, SubscriptionProduct group)
        {
            Plan stripePlan = await _stripe.GetPlanByIdAsync(subscription.StripeId);
            if (stripePlan != null)
            {
                PlanUpdateOptions planUpdate = new PlanUpdateOptions()
                {
                    Nickname = subscription.Name,
                    TrialPeriodDays = subscription.TrialPeriodDays,
                    Active = subscription.Public
                };
                await _stripe.UpdatePlanAsync(subscription.StripeId, planUpdate);
                await _stripe.MovePlanToProductAsync(subscription.StripeId, group.StripeId);
            }
            else
            {
                Plan response = await _stripe.CreatePlanAsync(subscription.Name, subscription.Amount, subscription.Currency, subscription.Interval, subscription.IntervalCount, subscription.TrialPeriodDays, group.StripeId);
                subscription.StripeId = response.Id;
            }
        }
        #endregion

        #region Stripe customer object
        public async Task<Customer> GetCustomerObjectAsync(string stripeId)
        {
            // Check if the user has a stripeId - if they do, we dont need to create them again, we can simply add a new card token to their account, or use an existing one maybe.
            if (!stripeId.IsSet())
            {
                return null;
            }

            try
            {
                Customer customer = await _stripe.GetCustomerByIdAsync(stripeId);
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
            {
                try
                {
                    List<Customer> customers = await _stripe.GetCustomersByEmailAsync(email);
                    return customers.ToList();
                }
                catch (StripeException)
                {
                    return new List<Customer>();
                }
            }
            else
            {
                return new List<Customer>();
            }
        }
        #endregion

        #region User Subscriptions
        public async Task<UserSubscriptionListModel> GetUserSubscriptionsAsync(UserSubscriptionListModel model)
        {
            IQueryable<UserSubscription> users = _db.UserSubscriptions
                .Include(us => us.User)
                .Include(us => us.Subscription).ThenInclude(s => s.SubscriptionProduct);

            if (model.SubscriptionPlanId.HasValue)
            {
                users = users.Where(u => u.SubscriptionId == model.SubscriptionPlanId);
            }

            if (model.Subscription.IsSet())
            {
                users = users.Where(u => u.StripeId == model.Subscription);
            }

            if (model.Linked)
            {
                users = users.Where(u => u.User != null);
            }

            if (model.Status.IsSet())
            {
                if (model.Status == "currently-active")
                    users = users.Where(u => u.Status == "active" || u.Status == "trialing");
                else
                    users = users.Where(u => u.Status == model.Status);
            }

            // search the collection
            if (!string.IsNullOrEmpty(model.Search))
            {

                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                users = users.Where(n => searchTerms.Any(s => n.User.FirstName != null && n.User.FirstName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.User.LastName != null && n.User.LastName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.User.StripeId != null && n.User.StripeId.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.User.Email != null && n.User.Email.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }


            // sort the collection and then output it.
            if (!string.IsNullOrEmpty(model.Order))
            {
                switch (model.Order)
                {
                    case "NextPeriodDate":
                        users = users.OrderBy(n => n.CurrentPeriodEnd);
                        break;
                    case "UserName":
                        users = users.OrderBy(n => n.User.UserName);
                        break;
                    case "Email":
                        users = users.OrderBy(n => n.User.Email);
                        break;
                    case "LastName":
                        users = users.OrderBy(n => n.User.LastName);
                        break;
                    case "LastLogOn":
                        users = users.OrderByDescending(n => n.User.LastLogOn);
                        break;

                    case "NextPeriodDateDesc":
                        users = users.OrderByDescending(n => n.CurrentPeriodEnd);
                        break;
                    case "UserNameDesc":
                        users = users.OrderByDescending(n => n.User.UserName);
                        break;
                    case "EmailDesc":
                        users = users.OrderByDescending(n => n.User.Email);
                        break;
                    case "LastNameDesc":
                        users = users.OrderByDescending(n => n.User.LastName);
                        break;

                    default:
                        users = users.OrderByDescending(n => n.User.CreatedOn).ThenBy(n => n.Id);
                        break;
                }
            }
            await model.ReloadAsync(users);
            return model;
        }
        public async Task<UserSubscription> GetUserSubscriptionByIdAsync(int id)
        {
            return await _db.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Subscription)
                .SingleOrDefaultAsync(c => c.Id == id);
        }
        public async Task<UserSubscription> GetUserSubscriptionByStripeIdAsync(string stripeId)
        {
            return await _db.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Subscription)
                .SingleOrDefaultAsync(c => c.StripeId == stripeId);
        }
        public async Task<UserSubscription> CreateUserSubscriptionAsync(int planId, string stripeToken, string cardId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            // Load the subscription plan.
            Models.Subscription subscription = await GetSubscriptionPlanByIdAsync(planId);

            // Get the stripe subscription plan object.
            Plan plan = await _stripe.GetPlanByIdAsync(subscription.StripeId);

            // Check for customer or throw, but allow it to be null.
            Customer customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
            {
                throw new Exception("There was a problem loading the customer object.");
            }

            // The object for the new user subscription to be recieved from stripe.
            Stripe.Subscription newSubscription = null;

            // if the user has provided a cardId to use, then let's try and use that!
            if (cardId.IsSet())
            {
                if (customer == null)
                {
                    throw new Exception("There is no customer account associated with this user.");
                }

                // set the card as the default for the user, then subscribe the user.
                await _stripe.SetDefaultCardAsync(customer.Id, cardId);

                // check if the user is already on a subscription, if so, update that.
                Stripe.Subscription sub = (await _stripe.GetSusbcriptionsByCustomerIdAsync(user.StripeId)).FirstOrDefault(s => s.Plan.Id == plan.Id && s.Status != "canceled");
                if (sub != null)
                {
                    // there is an existing subscription, which is not cancelleed and matches the plan. BAIL OUT!                   
                    throw new Exception("There is already a matching active subscription to this plan.");
                }
                else
                {
                    // finally, add the user to the NEW subscription.
                    newSubscription = await _stripe.SubscribeUserAsync(customer.Id, subscription.StripeId);
                }
            }
            else
            {
                // if not, then the user must have supplied a token
                Stripe.Token stripeTokenObj = await _stripe.TokenService.GetAsync(stripeToken);
                if (stripeTokenObj == null)
                {
                    throw new Exception("The payment method token was invalid.");
                }

                // Check if the customer object exists.
                if (customer == null)
                {
                    // if it does not, create it, add the card and subscribe the plan.
                    customer = await _stripe.CreateCustomerAsync(user, stripeToken);
                    user.StripeId = customer.Id;
                    await UpdateUserAsync(user);
                    newSubscription = await _stripe.SubscribeUserAsync(user.StripeId, plan.Id);
                }
                else
                {
                    // check if the user is already on a subscription, if so, update that.
                    Stripe.Subscription sub = (await _stripe.GetSusbcriptionsByCustomerIdAsync(user.StripeId)).FirstOrDefault(s => s.Plan.Id == plan.Id && s.Status != "canceled");
                    if (sub != null)
                    {
                        // there is an existing subscription, which is not cancelleed and matches the plan. BAIL OUT!                   
                        throw new Exception("There is already a matching active subscription to this plan.");
                    }
                    else
                    {
                        // finally, add the user to the NEW subscription, using the new card as the charge source.
                        Card source = await _stripe.CreateCardAsync(customer.Id, stripeToken);

                        // set the card as the default for the user, then subscribe the user.
                        await _stripe.SetDefaultCardAsync(customer.Id, source.Id);

                        newSubscription = await _stripe.SubscribeUserAsync(customer.Id, plan.Id);
                    }
                }
            }

            // We got this far, let's add the detail to the local DB.
            if (newSubscription == null)
            {
                throw new Exception("The new subscription was not created correctly, please try again.");
            }

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
            Customer customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
            {
                throw new Exception("There was a problem loading the customer object.");
            }

            if (customer.DefaultSourceId.IsSet() || customer.Sources?.Count() > 0)
            {
                // there is a payment source - continue                  
                // Load the plan from stripe, then add to the user's subscription.
                Plan plan = await _stripe.GetPlanByIdAsync(subscription.StripeId);
                if (customer == null)
                {
                    throw new Exception("There was a problem loading the subscription plan object.");
                }

                Stripe.Subscription sub = await _stripe.UpdateSubscriptionAsync(userSub.StripeId, plan);
                userSub = userSub.UpdateFromStripe(sub);
                userSub.Subscription = subscription;
                userSub.SubscriptionId = subscription.Id;
                await UpdateUserAsync(user);
            }
            else
            {
                throw new Exception("There is no currently active payment source on the account, please add one before upgrading.");
            }

            return userSub;
        }
        public async Task<UserSubscription> CancelUserSubscriptionAsync(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            // Check for customer or throw.
            Customer customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
            {
                throw new Exception("There was a problem loading the customer object.");
            }

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _stripe.CancelSubscriptionAsync(userSub.StripeId, true);
            userSub = userSub.UpdateFromStripe(sub);
            await UpdateUserAsync(user);
            return userSub;
        }
        public async Task<UserSubscription> RemoveUserSubscriptionAsync(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            // Check for customer or throw.
            Customer customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
            {
                throw new Exception("There was a problem loading the customer object.");
            }

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _stripe.CancelSubscriptionAsync(userSub.StripeId, false);
            userSub = userSub.UpdateFromStripe(sub);
            await UpdateUserAsync(user);
            return userSub;
        }
        public async Task<UserSubscription> ReactivateUserSubscriptionAsync(int userSubscriptionId)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetCurrentUserAsync();

            // Check for customer or throw.
            Customer customer = await GetCustomerObjectAsync(user.StripeId);
            if (customer == null)
            {
                throw new Exception("There was a problem loading the customer object.");
            }

            // Check for subscription or throw.
            UserSubscription userSub = GetUserSubscription(user, userSubscriptionId);

            Stripe.Subscription sub = await _stripe.GetSusbcriptionByIdAsync(userSub.StripeId);

            sub = await _stripe.UpdateSubscriptionAsync(sub.Id, sub.Plan);
            userSub = userSub.UpdateFromStripe(sub);
            await UpdateUserAsync(user);
            return userSub;
        }
        public async Task<UserSubscription> SyncUserSubscriptionAsync(int id)
        {
            var userSubscription = await GetUserSubscriptionByIdAsync(id);
            if (!userSubscription.StripeId.IsSet())
                throw new Exception("Could not sync with stripe, no stripe Id set for the user subscription.");

            var stripeSub = await _stripe.GetSusbcriptionByIdAsync(userSubscription.StripeId);
            if (stripeSub == null)
                throw new Exception("Could not sync with stripe, failed to load a stripe subscription object. If it has been deleted without syncing to the local subscription, you can remove this from the site.");

            // check the plan is correc'
            var stripePlan = await GetSubscriptionPlanByStripeIdAsync(stripeSub.Plan.Id);
            if (userSubscription.SubscriptionId == stripePlan.Id)
            {
                // Plan wasnt loaded correctly from the sub, try and load it from the stripe id.
                userSubscription.SubscriptionId = stripePlan.Id;
            }
            userSubscription = userSubscription.UpdateFromStripe(stripeSub);

            await _db.SaveChangesAsync();
            return userSubscription;
        }
        #endregion

        #region UserSubscription Helpers
        private UserSubscription GetUserSubscription(ApplicationUser user, int userSubscriptionId)
        {
            UserSubscription subscription = user.Subscriptions.Where(us => us.Id == userSubscriptionId).FirstOrDefault();
            if (subscription == null)
            {
                throw new Exception("Could not load the subscription for the user.");
            }

            return subscription;
        }
        private UserSubscription GetUserSubscriptionByStripeId(ApplicationUser user, string stripeId)
        {
            UserSubscription subscription = user.Subscriptions.Where(us => us.StripeId == stripeId).FirstOrDefault();
            if (subscription == null)
            {
                throw new Exception("Could not load the subscription for the user.");
            }

            return subscription;
        }
        #endregion

        #region WebHooks
        public async Task ConfirmSubscriptionObjectAsync(Stripe.Subscription created, DateTime? eventTime)
        {
            ApplicationUser user = await GetUserByStripeIdAsync(created.CustomerId);
            if (user == null)
            {
                throw new Exception($"Could not locate user from Stripe id: {created.CustomerId}");
            }

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, created.Id);
            if (userSub == null)
            {
                throw new Exception($"Could not locate user's subscription from Stripe id: {created.Id}");
            }

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.
            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
            {
                return;
            }

            userSub = userSub.UpdateFromStripe(created);
            userSub.Confirmed = true;
            userSub.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        public async Task UpdateSubscriptionObjectAsync(Stripe.Subscription updated, DateTime? eventTime)
        {
            ApplicationUser user = await GetUserByStripeIdAsync(updated.CustomerId);
            if (user == null)
            {
                throw new Exception($"Could not locate user from Stripe id: {updated.CustomerId}");
            }

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, updated.Id);
            if (userSub == null)
            {
                throw new Exception($"Could not locate user's subscription from Stripe id: {updated.Id}");
            }

            Models.Subscription plan = await GetSubscriptionPlanByStripeIdAsync(updated.Plan.Id);
            if (plan == null)
            {
                throw new Exception($"Could not locate subscription plan object from Stripe id: {updated.Plan.Id}");
            }

            if (userSub.SubscriptionId != plan.Id)
            {
                userSub.SubscriptionId = plan.Id;
            }

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.

            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
            {
                return;
            }

            userSub = userSub.UpdateFromStripe(updated);
            userSub.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        public async Task RemoveUserSubscriptionObjectAsync(Stripe.Subscription deleted, DateTime? eventTime)
        {
            ApplicationUser user = await GetUserByStripeIdAsync(deleted.CustomerId);
            if (user == null)
            {
                throw new Exception($"Could not locate user from Stripe id: {deleted.CustomerId}");
            }

            UserSubscription userSub = GetUserSubscriptionByStripeId(user, deleted.Id);
            if (userSub == null)
            {
                throw new Exception($"Could not locate user's subscription from Stripe id: {deleted.Id}");
            }

            // Check the timestamp of the event, with the last update of the object
            // If this was updated last before the event, therefore the event is valid and should be applied.
            if (eventTime.HasValue && userSub.LastUpdated > eventTime)
            {
                return;
            }

            userSub = userSub.UpdateFromStripe(deleted);
            userSub.Deleted = true;
            userSub.DeletedAt = DateTime.Now;
            userSub.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        #endregion

        #region Statistics
        public async Task<object> GetStatisticsAsync()
        {
            int totalUsers = await _db.Users.CountAsync();
            int totalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
            var data = await _db.Users.Select(c => new { date = c.CreatedOn.Date, month = c.CreatedOn.Month }).ToListAsync();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });

            List<KeyValuePair<string, int>> days = new List<KeyValuePair<string, int>>();
            foreach (DateTime day in DateTimeExtensions.EachDay(DateTime.Now.AddDays(-89), DateTime.Now))
            {
                var dayvalue = createdByDate.SingleOrDefault(c => c.name == day.Date);
                int count = dayvalue != null ? dayvalue.count : 0;
                days.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));

            }

            List<KeyValuePair<string, int>> months = new List<KeyValuePair<string, int>>();
            for (DateTime dt = DateTime.Now.AddMonths(-11); dt <= DateTime.Now; dt = dt.AddMonths(1))
            {
                var monthvalue = createdByMonth.SingleOrDefault(c => c.name == dt.Month);
                int count = monthvalue != null ? monthvalue.count : 0;
                months.Add(new KeyValuePair<string, int>(dt.ToString("dd MMM"), count));
            }

            return new { totalUsers, totalAdmins, days, months };
        }
        public async Task<object> GetSubscriptionStatisticsAsync()
        {
            int total = await _db.UserSubscriptions.CountAsync();
            int trials = await _db.UserSubscriptions.Where(c => c.Status == "trialing").CountAsync();
            int active = await _db.UserSubscriptions.Where(c => c.Status == "active").CountAsync();

            var data = await _db.UserSubscriptions.Where(c => c.Created.HasValue).Select(c => new { date = c.Created.Value.Date, month = c.Created.Value.Month }).ToListAsync();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });

            List<KeyValuePair<string, int>> days = new List<KeyValuePair<string, int>>();
            foreach (DateTime day in DateTimeExtensions.EachDay(DateTime.Now.AddDays(-89), DateTime.Now))
            {
                var dayvalue = createdByDate.SingleOrDefault(c => c.name == day.Date);
                int count = dayvalue != null ? dayvalue.count : 0;
                days.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));

            }

            List<KeyValuePair<string, int>> months = new List<KeyValuePair<string, int>>();
            for (DateTime dt = DateTime.Now.AddMonths(-11); dt <= DateTime.Now; dt = dt.AddMonths(1))
            {
                var monthvalue = createdByMonth.SingleOrDefault(c => c.name == dt.Month);
                int count = monthvalue != null ? monthvalue.count : 0;
                months.Add(new KeyValuePair<string, int>(dt.ToString("MMMM, yyyy"), count));
            }

            return new { total, trials, active, days, months };
        }
        #endregion

        #region Obsolete
        [Obsolete("Use _userManager.GetUserSubscriptionView(ClaimsPrincipal principal) from now on.", true)]
        public AccountInfo LoadAccountInfo(string userId)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
