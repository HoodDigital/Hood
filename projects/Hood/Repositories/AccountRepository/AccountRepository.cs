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
        public async Task<ApplicationUser> CreateLocalUserForCustomerObject(Customer customer)
        {
            // a user with this email address does not exist on the site, let's create one so they can access their account.
            var newUser = new ApplicationUser
            {
                UserName = customer.Email,
                Email = customer.Email,
                FirstName = customer.Name.ToFirstName(),
                LastName = customer.Name.ToLastName(),
                PhoneNumber = customer.Phone,
                Anonymous = false,
                CreatedOn = DateTime.Now,
                StripeId = customer.Id
            };
            var result = await _userManager.CreateAsync(newUser);
            if (result.Succeeded)
                return newUser;
            return null;
        }
        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _db.Update(user);
            await _db.SaveChangesAsync();
        }
        public async Task DeleteUserAsync(string userId, System.Security.Claims.ClaimsPrincipal adminUser)
        {
            UserProfile siteOwner = Engine.Settings.SiteOwner;

            if (userId == siteOwner.Id)
            {
                throw new Exception("You cannot delete the site owner, you must assign a new site owner (from the site owner account) before you can delete this account.");
            }

            ApplicationUser user = await _db.Users
                .Include(u => u.Content)
                .Include(u => u.Properties)
                .Include(u => u.Forums)
                .Include(u => u.Topics)
                .Include(u => u.Posts)
                .Include(u => u.Posts)
                .Include(u => u.Subscriptions)
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

                UserProfile userProfile = await GetUserProfileByIdAsync(user.Id);
                if (userProfile.ActiveSubscriptions != null)
                {
                    if (userProfile.ActiveSubscriptions.Count > 0)
                    {
                        if (adminUser.GetUserId() != user.Id)
                            throw new Exception("You cannot delete a user with active subscriptions, cancel them or delete them before deleting the user.");
                        else
                            throw new Exception("You have active subscriptions, cancel or delete them before deleting your account.");
                    }
                }

                user.Subscriptions.ForEach(p => _db.Entry(p).State = EntityState.Deleted);

                if (userProfile.StripeId.IsSet())
                {
                    await _stripe.DeleteCustomerAsync(userProfile.StripeId);
                }

                await _db.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
            }
            else
            {
                throw new Exception("You do not have permission to delete this user.");
            }
        }
        public async Task<MediaDirectory> GetDirectoryAsync(string id)
        {
            MediaDirectory directory = await _db.MediaDirectories.SingleOrDefaultAsync(md => md.OwnerId == id && md.Type == DirectoryType.User);
            if (directory == null)
            {
                MediaDirectory userDirectory = await _db.MediaDirectories.SingleOrDefaultAsync(md => md.Slug == MediaManager.UserDirectorySlug && md.Type == DirectoryType.System);
                ApplicationUser user = await GetUserByIdAsync(id);
                if (user == null)
                {
                    throw new Exception("No user found to add/get directory for.");
                }

                directory = new MediaDirectory()
                {
                    OwnerId = id,
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
            IQueryable<SubscriptionProduct> query = _db.SubscriptionProducts
                .Include(g => g.Subscriptions)
                .AsQueryable();

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
            SubscriptionProduct subscriptionProduct = new SubscriptionProduct()
            {
                DisplayName = name,
                Slug = name.ToSeoUrl(),
                StripeId = stripeId,
                CreatedBy = Engine.Account == null ? "Stripe" : Engine.Account.Id,
                Created = DateTime.Now,
                LastEditedBy = Engine.Account == null ? "Stripe" : Engine.Account.UserName,
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
        public async Task<SubscriptionProduct> DeleteSubscriptionProductAsync(int id)
        {
            SubscriptionProduct subscriptionProduct = await GetSubscriptionProductByIdAsync(id);
            if (subscriptionProduct == null)
            {
                throw new Exception("Could not find the product group.");
            }

            if (subscriptionProduct.Subscriptions != null && subscriptionProduct.Subscriptions.Count() > 0)
            {
                throw new Exception("You cannot remove a subscription product group which contains plans. Move them to a new product group or delete them before you remove the product group.");
            }

            Product stripePlan = await _stripe.GetProductByIdAsync(subscriptionProduct.StripeId);
            if (stripePlan != null)
            {
                StripeList<Plan> subcount = await _stripe.PlanService.ListAsync(new PlanListOptions() { Limit = 1, ProductId = subscriptionProduct.StripeId });
                if (subcount != null && subcount.Count() > 0)
                {
                    throw new Exception("You cannot remove a subscription product group which contains plans (on Stripe). Move them to a new product group or delete them before you remove the product group.");
                }

                await _stripe.DeleteProductAsync(subscriptionProduct.StripeId);
            }

            _db.SaveChanges();
            _db.Entry(subscriptionProduct).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            return subscriptionProduct;
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
                {
                    product = await GetSubscriptionProductByStripeIdAsync(stripeId);
                }
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
                product.LastEditedBy = Engine.Account == null ? "Stripe" : Engine.Account.UserName;
                product.LastEditedOn = DateTime.Now;
                await UpdateSubscriptionProductAsync(product);
            }

            // now go through all local plans associated
            product = await GetSubscriptionProductByIdAsync(id.Value);
            if (product.Subscriptions.Count > 0)
            {
                foreach (Models.Subscription s in product.Subscriptions)
                {
                    await SyncSubscriptionPlanAsync(s.Id, s.StripeId);
                }
            }

            IEnumerable<Plan> plans = await _stripe.GetAllPlansAsync(new PlanListOptions() { ProductId = product.StripeId });
            foreach (Plan p in plans)
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
        public async Task<Models.Subscription> DeleteSubscriptionPlanAsync(int id)
        {
            Models.Subscription subscription = _db.Subscriptions.Where(p => p.Id == id).FirstOrDefault();
            if (subscription == null)
            {
                throw new Exception("Could not find the subscription plan.");
            }

            Plan stripePlan = await _stripe.GetPlanByIdAsync(subscription.StripeId);
            if (stripePlan != null)
            {
                StripeList<Stripe.Subscription> subcount = await _stripe.SubscriptionService.ListAsync(new SubscriptionListOptions() { Limit = 1, PlanId = subscription.StripeId, Status = SubscriptionStatuses.Active });
                if (subcount != null && subcount.Count() > 0)
                {
                    throw new Exception("You cannot remove a subscription with active subscribers. Move them to a new subscription on Stripe or cancel them before you remove the plan.");
                }

                subcount = await _stripe.SubscriptionService.ListAsync(new SubscriptionListOptions() { Limit = 1, PlanId = subscription.StripeId, Status = SubscriptionStatuses.Trialing });
                if (subcount != null && subcount.Count() > 0)
                {
                    throw new Exception("You cannot remove a subscription with trial subscribers. Move them to a new subscription on Stripe or cancel them before you remove the plan.");
                }

                await _stripe.DeletePlanAsync(subscription.StripeId);
            }

            _db.SaveChanges();
            _db.Entry(subscription).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            return subscription;
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
                    SubscriptionProduct product = await _db.SubscriptionProducts.SingleOrDefaultAsync(c => c.StripeId == stripePlan.ProductId);
                    if (product == null)
                    {
                        product = await CreateSubscriptionProductAsync(stripePlan.Nickname, stripePlan.ProductId);
                    }
                    subscriptionPlan = new Models.Subscription()
                    {
                        Name = stripePlan.Nickname,
                        StripeId = stripePlan.Id,
                        CreatedBy = Engine.Account == null ? "Stripe" : Engine.Account.UserName,
                        Created = DateTime.Now,
                        LastEditedBy = Engine.Account == null ? "Stripe" : Engine.Account.UserName,
                        LastEditedOn = DateTime.Now,
                        Addon = false,
                        Public = stripePlan.Active,
                        Amount = (int)stripePlan.Amount,
                        Currency = stripePlan.Currency,
                        Interval = stripePlan.Interval,
                        IntervalCount = (int)stripePlan.IntervalCount,
                        NumberAllowed = 1,
                        SubscriptionProductId = product.Id
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
            SubscriptionProduct product = await _db.SubscriptionProducts.SingleOrDefaultAsync(c => c.Id == subscriptionPlan.SubscriptionProductId);
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
                {
                    await _stripe.CreateProductAsync(product.DisplayName);
                }

                Product stripeProduct = await _stripe.GetProductByIdAsync(product.StripeId);
                if (stripeProduct == null)
                {
                    await _stripe.CreateProductAsync(product.DisplayName);
                }

                if (stripeProduct != null)
                {
                    product.StripeId = stripeProduct.Id;
                }

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
            subscriptionPlan.LastEditedBy = Engine.Account == null ? "Stripe" : Engine.Account.UserName;
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
        public async Task<Customer> GetOrCreateStripeCustomerForUser(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            // Check if the user has a stripeId - if they do, we dont need to create them again, we can simply add a new card token to their account, or use an existing one maybe.
            Customer customer;
            if (user.StripeId.IsSet())
            {
                customer = await _stripe.GetCustomerByIdAsync(user.StripeId);
                if (customer != null)
                    return customer;
            }
            customer = await _stripe.CreateCustomerAsync(user);
            user.StripeId = customer.Id;
            await UpdateUserAsync(user);
            return customer;
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

            if (model.UserId.IsSet())
            {
                users = users.Where(u => u.UserId == model.UserId);
            }

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
                {
                    users = users.Where(u => u.Status == "active" || u.Status == "trialing");
                }
                else
                {
                    users = users.Where(u => u.Status == model.Status);
                }
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
        public async Task<UserSubscription> CreateUserSubscription(int planId, string userId, Stripe.Subscription newSubscription)
        {
            // Load user object and clear cache.
            ApplicationUser user = await GetUserByIdAsync(userId);
            UserSubscription newUserSub = new UserSubscription();
            newUserSub = newUserSub.UpdateFromStripe(newSubscription);
            newUserSub.StripeId = newSubscription.Id;
            newUserSub.CustomerId = user.StripeId;
            newUserSub.UserId = user.Id;
            newUserSub.SubscriptionId = planId;
            await _db.UserSubscriptions.AddAsync(newUserSub);
            _db.SaveChanges();
            return newUserSub;
        }
        public async Task<UserSubscription> DeleteUserSubscriptionAsync(int id)
        {
            UserSubscription subscription = await GetUserSubscriptionByIdAsync(id);

            if (subscription == null)
            {
                throw new Exception("Could not find the subscription.");
            }

            if (subscription.Status == Stripe.SubscriptionStatuses.Active || subscription.Status == SubscriptionStatuses.Trialing)
            {
                var stripeUrl = string.Format("https://dashboard.stripe.com{0}/subscriptions/{1}", Engine.Settings.Billing.EnableStripeTestMode ? "/test" : "", subscription.StripeId);
                throw new Exception($"You cannot delete an active or trialing subscription, you will need to cancel it first.<br /><a class='font-weight-bold' target='_blank' href='{stripeUrl}'>Modify the subscription on Stripe</a>");
            }

            subscription = await GetUserSubscriptionByIdAsync(id);
            _db.Entry(subscription).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            return subscription;
        }
        public async Task<UserSubscription> CancelUserSubscriptionAsync(int userSubscriptionId, bool cancelAtPeriodEnd = true, bool invoiceNow = false, bool prorate = false)
        {
            // Check for subscription or throw.
            UserSubscription userSub = await GetUserSubscriptionByIdAsync(userSubscriptionId);
            if (userSub == null)
            {
                throw new Exception("There was a problem loading the subscription object.");
            }

            Stripe.Subscription sub = await _stripe.CancelSubscriptionAsync(userSub.StripeId, cancelAtPeriodEnd, invoiceNow, prorate);
            userSub = userSub.UpdateFromStripe(sub);
            await _db.SaveChangesAsync();
            return userSub;
        }
        public async Task<UserSubscription> ReactivateUserSubscriptionAsync(int userSubscriptionId)
        {
            // Check for subscription or throw.
            UserSubscription userSub = await GetUserSubscriptionByIdAsync(userSubscriptionId);
            if (userSub == null)
            {
                throw new Exception("There was a problem loading the subscription object.");
            }

            Stripe.Subscription sub = await _stripe.ReactivateSubscriptionAsync(userSub.StripeId);
            userSub = userSub.UpdateFromStripe(sub);
            await _db.SaveChangesAsync();
            return userSub;
        }
        public async Task<UserSubscription> SwitchUserSubscriptionAsync(int subscriptionId, int newPlanId)
        {
            UserSubscription userSub = await GetUserSubscriptionByIdAsync(subscriptionId);

            // Check for customer or throw.
            Customer customer = await GetOrCreateStripeCustomerForUser(userSub.StripeId);
            if (customer == null)
            {
                throw new Exception("There was a problem loading the customer object.");
            }

            if (customer.DefaultSourceId.IsSet() || customer.Sources?.Count() > 0)
            {
                // there is a payment source - continue                  
                // Load the plan from stripe, then add to the user's subscription.
                var newPlan = await GetSubscriptionPlanByIdAsync(newPlanId);
                if (newPlan == null)
                {
                    throw new Exception("There was a problem loading the subscription plan.");
                }

                Stripe.Subscription sub = await _stripe.SwitchSubscriptionPlanAsync(userSub.StripeId, newPlan.StripeId);
                userSub = userSub.UpdateFromStripe(sub);
                userSub.SubscriptionId = newPlan.Id;
                await _db.SaveChangesAsync();
                return userSub;
            }
            else
            {
                throw new Exception("There is no currently active payment source on the account, please add one before upgrading.");
            }
        }
        public async Task<UserSubscription> SyncUserSubscriptionAsync(int? id, string stripeId)
        {
            UserSubscription userSubscription = null;
            if (id.HasValue)
            {
                userSubscription = await GetUserSubscriptionByIdAsync(id.Value);
            }

            Stripe.Subscription stripeSub = null;
            if (stripeId.IsSet())
            {
                stripeSub = await _stripe.GetSusbcriptionByIdAsync(stripeId);
            }
            else if (userSubscription != null && userSubscription.StripeId.IsSet())
            {
                stripeSub = await _stripe.GetSusbcriptionByIdAsync(userSubscription.StripeId);
            }

            if (userSubscription == null && stripeSub == null)
            {
                throw new Exception("Could not sync, neither a local or a stripe object could be loaded.");
            }

            if (userSubscription == null && stripeSub != null)
            {
                // try to get the local from the stripe sub's returned Id.
                userSubscription = await GetUserSubscriptionByStripeIdAsync(stripeSub.Id);
                if (userSubscription == null)
                {
                    // if the sub is active/trialing, and doesn't exist locally it needs to be created.
                    if (stripeSub.Status == SubscriptionStatuses.Active || stripeSub.Status == SubscriptionStatuses.Trialing)
                    {
                        // find an associated user.
                        var user = await GetUserByStripeIdAsync(stripeSub.CustomerId);
                        if (user == null)
                        {
                            var customer = await _stripe.GetCustomerByIdAsync(stripeSub.CustomerId);
                            user = await CreateLocalUserForCustomerObject(customer);
                            if (user == null)
                            {
                                throw new AlertedException($"Stripe customer could not be linked to a local account, and the create user function failed.");
                            }
                        }
                        var subscriptionPlan = await SyncSubscriptionPlanAsync(null, stripeSub.Plan.Id);

                        userSubscription = new UserSubscription();
                        userSubscription.UpdateFromStripe(stripeSub);
                        userSubscription.StripeId = stripeSub.Id;
                        userSubscription.CustomerId = stripeSub.CustomerId;
                        userSubscription.UserId = user.Id;
                        userSubscription.SubscriptionId = subscriptionPlan.Id;
                        _db.UserSubscriptions.Add(userSubscription);
                    }
                }
                else
                {
                    userSubscription = await UpdateSubscriptionFromStripe(userSubscription, stripeSub);
                }
            }
            else if (userSubscription != null && stripeSub == null)
            {
                // the subscription doesnt exist on stripe. Let's remove the local version.
                userSubscription = await DeleteUserSubscriptionAsync(userSubscription.Id);
            }
            else
            {
                userSubscription = await UpdateSubscriptionFromStripe(userSubscription, stripeSub);
            }
            await _db.SaveChangesAsync();

            return userSubscription;
        }
        private async Task<UserSubscription> UpdateSubscriptionFromStripe(UserSubscription userSubscription, Stripe.Subscription stripeSub)
        {
            if (stripeSub.Plan == null)
                throw new AlertedException($"Could not load a Stripe plan from the subscription update for Id {userSubscription.StripeId}. It may have multiple items attached to it, this is not supported by this system currently. Remove multiple items to re-sync this subscription correctly.");
            var subscriptionPlan = await SyncSubscriptionPlanAsync(null, stripeSub.Plan.Id);
            if (stripeSub.Plan == null)
                throw new AlertedException($"Could not load a Stripe plan from the subscription update for Id {userSubscription.StripeId}.");
            userSubscription.SubscriptionId = subscriptionPlan.Id;
            userSubscription = userSubscription.UpdateFromStripe(stripeSub);
            return userSubscription;
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

    }
}
