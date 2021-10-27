using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public AccountRepository(
            HoodDbContext db,
            IHttpContextAccessor context,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _contextAccessor = context;
            _userManager = userManager;
        }

        #region User Get/Update/Delete
        private IQueryable<ApplicationUser> UserQuery
        {
            get
            {
                IQueryable<ApplicationUser> query = _db.Users
                    .Include(u => u.Addresses)
                    .Include(u => u.AccessCodes);
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
            ApplicationUser user = await _db.Users
                .Include(u => u.Content)
                .Include(u => u.Properties)
                .Include(u => u.Addresses)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user.Email == Engine.Configuration.SuperAdminEmail)
            {
                throw new Exception("You cannot delete the site owner account, the owner is set via an environment variable and cannot be changed from the admin area.");
            }

            ApplicationUser siteOwner = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == Engine.Configuration.SuperAdminEmail);
            if (siteOwner == null) 
            {
                throw new Exception("Could not load the owner account, check your settings, the owner is set via an environment variable and cannot be changed from the admin area.");
            }

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

                // Set any site content as owned by the site owner, instead of the user.
                user.Content.ForEach(c => c.AuthorId = siteOwner.Id);
                user.Properties.ForEach(p => p.AgentId = siteOwner.Id);

                _db.Logs.Where(l => l.UserId == userId).ForEach(f => f.UserId = siteOwner.Id);

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
        public async Task<IUserListModel> GetUserProfilesAsync(IUserListModel model, IQueryable<UserProfile> query = null)
        {
            if (query == null) 
            {                            
                query = _db.UserProfiles.AsQueryable();
            }

            if (model.Role.IsSet())
            {
                query = query.Where(q => q.RoleIds.Contains(model.Role));
            }

            if (model.RoleIds != null && model.RoleIds.Count > 0)
            {
                query = query.Where(q => q.RoleIds != null && model.RoleIds.ToArray().Any(m => q.RoleIds.Contains(m)));
            }

            if (!string.IsNullOrEmpty(model.Search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(model.Search) ||
                    u.Email.Contains(model.Search) ||
                    u.DisplayName.Contains(model.Search) ||
                    u.FirstName.Contains(model.Search) ||
                    u.LastName.Contains(model.Search)
                );
            }

            if (model.Active) {
                query = query.Where(q => q.Active);
            }            

            if (model.Inactive) {
                query = query.Where(q => !q.Active);
            }            
            
            if (model.PhoneUnconfirmed) {                
                query = query.Where(q => !q.PhoneNumberConfirmed);
            }            
            
            if (model.EmailUnconfirmed) {                
                query = query.Where(q => !q.EmailConfirmed);
            }

            if (model.Unused) {                
                query = query.Where(q => q.LastLoginLocation == null || q.LastLoginLocation == null || q.LastLogOn == DateTime.MinValue);
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
            Models.Address address = await GetAddressByIdAsync(id);
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

        #region Statistics
        public async Task<UserStatistics> GetStatisticsAsync()
        {
            int totalUsers = await _db.Users.CountAsync();
            int totalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
            var data = await _db.Users.Where(p => p.CreatedOn >= DateTime.Now.AddYears(-1)).Select(c => new { date = c.CreatedOn.Date, month = c.CreatedOn.Month }).ToListAsync();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });

            List<KeyValuePair<string, int>> days = new List<KeyValuePair<string, int>>();
            foreach (DateTime day in DateTimeExtensions.EachDay(DateTime.UtcNow.AddDays(-89), DateTime.UtcNow))
            {
                var dayvalue = createdByDate.SingleOrDefault(c => c.name == day.Date);
                int count = dayvalue != null ? dayvalue.count : 0;
                days.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));

            }

            List<KeyValuePair<string, int>> months = new List<KeyValuePair<string, int>>();
            for (DateTime dt = DateTime.UtcNow.AddMonths(-11); dt <= DateTime.UtcNow; dt = dt.AddMonths(1))
            {
                var monthvalue = createdByMonth.SingleOrDefault(c => c.name == dt.Month);
                int count = monthvalue != null ? monthvalue.count : 0;
                months.Add(new KeyValuePair<string, int>(dt.ToString("dd MMM"), count));
            }

            return new UserStatistics(totalUsers, totalAdmins, days, months);
        }
        #endregion

    }

    public class UserStatistics
    {
        public UserStatistics(int totalUsers, int totalAdmins, List<KeyValuePair<string, int>> days, List<KeyValuePair<string, int>> months)
        {
            TotalUsers = totalUsers;
            TotalAdmins = totalAdmins;
            Days = days;
            Months = months;
        }

        public int TotalUsers { get; }
        public int TotalAdmins { get; }
        public List<KeyValuePair<string, int>> Days { get; }
        public List<KeyValuePair<string, int>> Months { get; }
    }
}
