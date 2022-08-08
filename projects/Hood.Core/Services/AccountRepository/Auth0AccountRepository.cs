using Hood.Contexts;
using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class Auth0AccountRepository : IAuth0AccountRepository
    {
        protected readonly Auth0IdentityContext _db;
        protected readonly HoodDbContext _hoodDb;
        protected readonly IAuth0Service _auth0;

        private IQueryable<Auth0User> Users { get { return _db.Users.Include(u => u.ConnectedAuth0Accounts).Include(u => u.UserProfile); } }
        private IQueryable<Auth0Role> Roles { get { return _db.Set<Auth0Role>(); } }
        private IQueryable<Auth0UserRole> UserRoles { get { return _db.Set<Auth0UserRole>(); } }

        public Auth0AccountRepository()
        {
            _db = Engine.Services.Resolve<Auth0IdentityContext>();
            _hoodDb = Engine.Services.Resolve<HoodDbContext>();
            _auth0 = Engine.Services.Resolve<IAuth0Service>();
        }

        #region Account stuff                 
        public virtual async Task<Auth0User> GetUserByIdAsync(string id, bool track = true)
        {
            if (!id.IsSet())
            {
                return null;
            }

            IQueryable<Auth0User> query = Users;
            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(u => u.Id == id);
        }
        public virtual async Task<Auth0User> GetUserByEmailAsync(string email, bool track = true)
        {
            if (!email.IsSet())
            {
                return null;
            }

            IQueryable<Auth0User> query = Users;
            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(u => u.Email == email);
        }
        public virtual async Task<bool> CreateAsync(Auth0User user)
        {
            try
            {
                _db.Users.Add(user);
                return await _db.SaveChangesAsync() == 1;
            }
            catch (Exception)
            { }
            return false;
        }
        public async Task UpdateUserAsync(Auth0User user)
        {
            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }
        public async Task DeleteUserAsync(string localUserId, System.Security.Claims.ClaimsPrincipal adminUser)
        {
            var user = await PrepareUserForDelete(localUserId, adminUser);

            // go through all the auth0 accounts and remove them from the system.
            foreach (var account in user.ConnectedAuth0Accounts)
            {
                //remove from auth0
                if (Engine.Settings.Account.DeleteRemoteAccounts)
                {
                    await _auth0.DeleteUser(account.Id);
                }
                _db.Entry(account).State = EntityState.Deleted;
            }
            await _db.SaveChangesAsync();

            // now delete the user
            _db.Entry(user).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
        }
        public virtual async Task<MediaDirectory> GetDirectoryAsync(string id)
        {
            MediaDirectory directory = await _hoodDb.MediaDirectories.SingleOrDefaultAsync(md => md.OwnerId == id && md.Type == DirectoryType.User);
            if (directory == null)
            {
                MediaDirectory userDirectory = await _hoodDb.MediaDirectories.SingleOrDefaultAsync(md => md.Slug == MediaManager.UserDirectorySlug && md.Type == DirectoryType.System);
                Auth0User user = await GetUserByIdAsync(id);
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

        #region Auth0 Identities

        public async Task<Auth0User> GetUserByAuth0Id(string id)
        {
            var auth0user = await _db.Auth0Identities
                    .Include(au => au.User).ThenInclude(u => u.UserProfile)
                    .SingleOrDefaultAsync(au => au.Id == id);
            if (auth0user != null)
            {
                return auth0user.User;
            }
            return null;
        }

        public async Task<Auth0Identity> CreateLocalAuthIdentity(string fullAuthUserId, Auth0User user, string picture)
        {
            var authProviderName = fullAuthUserId.Split('|')[0];
            var authUserId = fullAuthUserId.Split('|')[1];
            Auth0.ManagementApi.Models.User newAuthUser = null;
            try
            {
                newAuthUser = await _auth0.GetUserById(fullAuthUserId);
            }
            catch (Auth0.Core.Exceptions.ErrorApiException)
            {
            }

            var primaryIdentity = user.GetPrimaryIdentity();
            if (newAuthUser == null && primaryIdentity != null)
            {
                newAuthUser = await _auth0.GetUserById(primaryIdentity.Id);
            }

            if (newAuthUser == null)
            {
                throw new Exception("Could not find the user on the remote service.");
            }

            var identity = newAuthUser.Identities.SingleOrDefault(i => i.UserId == authUserId);
            if (identity == null)
            {
                throw new Exception("Could not find the identity on the remote user profile.");
            }

            var newIdentity = new Auth0Identity(identity);

            if (primaryIdentity == null)
            {
                newIdentity.IsPrimary = true;
            }

            newIdentity.Id = fullAuthUserId;
            newIdentity.LocalUserId = user.Id;
            newIdentity.Picture = picture;

            _db.Add(newIdentity);
            await _db.SaveChangesAsync();
            return newIdentity;
        }

        public async Task DeleteLocalAuthIdentity(string id)
        {
            var userToRemove = _db.Auth0Identities.SingleOrDefault(u => u.Id == id);
            _db.Entry(userToRemove).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateLocalAuthIdentity(Auth0Identity user)
        {
            _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task<List<Auth0Identity>> GetUserAuth0IdentitiesById(string id)
        {
            return await _db.Auth0Identities
                .Where(u => u.LocalUserId == id)
                .ToListAsync();
        }

        #endregion

        #region Helpers

        protected virtual async Task<Auth0User> PrepareUserForDelete(string userId, System.Security.Claims.ClaimsPrincipal adminUser)
        {
            Auth0User user = await Users
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user.Email == Engine.Configuration.SuperAdminEmail)
            {
                throw new Exception("You cannot delete the site owner account, the owner is set via an environment variable and cannot be changed from the admin area.");
            }

            Auth0User siteOwner = await Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == Engine.Configuration.SuperAdminEmail);
            if (siteOwner == null)
            {
                throw new Exception("Could not load the owner account, check your settings, the owner is set via an environment variable and cannot be changed from the admin area.");
            }

            if (!adminUser.IsAdminOrBetter() && adminUser.GetLocalUserId() != user.Id)
            {
                throw new Exception("You do not have permission to delete this user.");
            }

            await _db.SaveChangesAsync();

            return user;
        }

        #endregion

        #region Profiles              
        public virtual async Task<UserProfile> GetUserProfileByIdAsync(string id)
        {
            UserProfile profile = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id);
            return profile;
        }
        public virtual async Task<UserListModel> GetUserProfilesAsync(UserListModel model, IQueryable<IUserProfile> query = null)
        {
            if (query == null)
            {
                query = _db.UserProfileViews.AsQueryable();
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
        public virtual async Task<UserProfileView<Auth0Role>> GetUserProfileViewById(string id)
        {
            UserProfileView<Auth0Role> profile = await _db.UserProfileViews.FirstOrDefaultAsync(u => u.Id == id);
            return profile;
        }
        public virtual async Task<UserListModel<UserProfileView<Auth0Role>>> GetUserProfileViewsAsync(UserListModel<UserProfileView<Auth0Role>> model)
        {
            var query = _db.UserProfileViews.AsQueryable();

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

            if (model.Active)
            {
                query = query.Where(q => q.Active);
            }

            if (model.Inactive)
            {
                query = query.Where(q => !q.Active);
            }

            if (model.PhoneUnconfirmed)
            {
                query = query.Where(q => !q.PhoneNumberConfirmed);
            }

            if (model.EmailUnconfirmed)
            {
                query = query.Where(q => !q.EmailConfirmed);
            }

            if (model.Unused)
            {
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

        public virtual async Task<Auth0User> UpdateProfileAsync(Auth0User user, IUserProfile profile)
        {
            foreach (PropertyInfo property in typeof(IUserProfile).GetProperties())
            {
                property.SetValue(user.UserProfile, property.GetValue(profile));
            }

            foreach (PropertyInfo property in typeof(IName).GetProperties())
            {
                property.SetValue(user.UserProfile, property.GetValue(profile));
            }

            foreach (PropertyInfo property in typeof(IJsonMetadata).GetProperties())
            {
                property.SetValue(user.UserProfile, property.GetValue(profile));
            }

            _db.Update(user);
            await _db.SaveChangesAsync();

            return user;
        }
        #endregion

        #region Roles
        public virtual async Task<RoleListModel<Auth0Role>> GetRolesAsync(RoleListModel<Auth0Role> model)
        {
            if (model == null)
            {
                model = new RoleListModel<Auth0Role>() { PageIndex = 0, PageSize = 50 };
            }
            var query = _db.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(model.Search))
            {
                query = query.Where(u => u.Name.Contains(model.Search));
            }

            switch (model.Order)
            {
                case "Name":
                    query = query.OrderBy(n => n.Name);
                    break;
                case "NameDesc":
                    query = query.OrderByDescending(n => n.Name);
                    break;
                default:
                    query = query.OrderBy(n => n.Name);
                    break;
            }

            await model.ReloadAsync(query);

            return model;
        }

        public virtual async Task<IList<Auth0Role>> GetRolesForUser(Auth0User user)
        {
            var userId = user.Id;
            var query = from userRole in UserRoles
                        join role in Roles on userRole.RoleId equals role.Id
                        where userRole.UserId.Equals(userId)
                        select role;
            return await query.ToListAsync();
        }
        public virtual async Task<IList<IUserProfile>> GetUsersInRole(string roleName)
        {
            var role = await GetRoleAsync(roleName);
            if (role != null)
            {
                var query = from userrole in UserRoles
                            join user in Users on userrole.UserId equals user.Id
                            where userrole.RoleId.Equals(role.Id)
                            select user.UserProfile;

                return await query.ToListAsync<IUserProfile>();
            }
            return new List<IUserProfile>();
        }
        public virtual async Task<bool> RoleExistsAsync(string role)
        {
            return await GetRoleAsync(role) != null;
        }
        public virtual async Task<Auth0Role> GetRoleAsync(string role)
        {
            return await _db.Roles.SingleOrDefaultAsync(r => r.NormalizedName == role.ToUpperInvariant());
        }
        public virtual async Task<Auth0Role> CreateRoleAsync(string role)
        {
            var roleObject = await GetRoleAsync(role);
            if (roleObject == null)
            {
                roleObject = new Auth0Role()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = role,
                    NormalizedName = role.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                _db.Roles.Add(roleObject);
                await _db.SaveChangesAsync();
            }
            return roleObject;
        }

        public virtual async Task DeleteRoleAsync(string role)
        {
            var Auth0Role = await GetRoleAsync(role);
            _db.Roles.Remove(Auth0Role);
            if (Auth0Role.RemoteId.IsSet())
            {
                try
                {
                    await _auth0.DeleteRole(Auth0Role.RemoteId);
                }
                catch (Exception)
                { }
            }
            await _db.SaveChangesAsync();
        }

        public virtual async Task<Response> AddUserToRolesAsync(Auth0User user, Auth0Role[] roles)
        {
            foreach (var ac in user.ConnectedAuth0Accounts)
            {
                foreach (Auth0Role role in roles)
                {
                    _db.UserRoles.Add(new Auth0UserRole()
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                }
                await _auth0.AssignRolesToUser(ac.Id, roles.Select(r => r.RemoteId).ToArray());
            }
            await _db.SaveChangesAsync();
            return new Response(true);
        }

        public virtual async Task<Response> RemoveUserFromRolesAsync(Auth0User user, Auth0Role[] roles)
        {
            foreach (var ac in user.ConnectedAuth0Accounts)
            {
                foreach (Auth0Role role in roles)
                {
                    var userRole = await FindUserRoleAsync(user.Id, role.Id);
                    if (userRole != null)
                    {
                        _db.UserRoles.Remove(userRole);
                    }
                    await _auth0.AssignRolesToUser(ac.Id, roles.Select(r => r.RemoteId).ToArray());
                }
            }
            await _db.SaveChangesAsync();
            return new Response(true);
        }

        protected Task<Auth0UserRole> FindUserRoleAsync(string userId, string roleId)
        {
            return _db.UserRoles.FindAsync(new object[] { userId, roleId }).AsTask();
        }

        public async Task SetupRolesAsync()
        {
            foreach (string role in Models.Roles.All)
            {
                if (!await RoleExistsAsync(role))
                {
                    await CreateRoleAsync(role);
                }
            }
        }

        public async Task UpdateRoleAsync(Auth0Role role)
        {
            _db.Entry(role).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        #endregion

        #region Statistics
        public virtual async Task<UserStatistics> GetStatisticsAsync()
        {
            int totalUsers = await _db.Users.CountAsync();
            int totalAdmins = (await GetUsersInRole("Admin")).Count;
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
}