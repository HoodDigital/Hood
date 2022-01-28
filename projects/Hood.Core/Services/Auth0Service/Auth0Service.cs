using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Hood.Caching;
using Hood.Core;
using Hood.Extensions;
using Hood.Identity;
using Hood.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using RestSharp;

namespace Hood.Services
{
    public class Auth0Service
    {
        protected IHoodCache _cache { get; set; }
        public Auth0Service()
        {
            _cache = Engine.Services.Resolve<IHoodCache>();
        }

        #region Data CRUD
        public async Task GetLocalAuth0User(string userId)
        {
            var db = Engine.Services.Resolve<HoodDbContext>();
            var userToRemove = db.Auth0Users.SingleOrDefault(u => u.UserId == userId);
            db.Entry(userToRemove).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await db.SaveChangesAsync();
        }


        public async Task<Auth0User> CreateLocalAuth0User(string authUserId, ApplicationUser user)
        {
            var newAuthUser = await GetUserById(authUserId);
            var db = Engine.Services.Resolve<HoodDbContext>();
            newAuthUser.LocalUserId = user.Id;
            if (newAuthUser.Identities.FirstOrDefault() != null)
            {
                newAuthUser.ProviderName = newAuthUser.Identities.FirstOrDefault().Provider;
            }
            else
            {
                newAuthUser.ProviderName = newAuthUser.UserId.Split('|')[0];
            }
            db.Add(newAuthUser);
            await db.SaveChangesAsync();
            return newAuthUser;
        }
        public async Task DeleteLocalAuth0User(string userId)
        {
            var db = Engine.Services.Resolve<HoodDbContext>();
            var userToRemove = db.Auth0Users.SingleOrDefault(u => u.UserId == userId);
            db.Entry(userToRemove).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        public async Task UpdateLocalAuth0User(Auth0User user)
        {
            var db = Engine.Services.Resolve<HoodDbContext>();
            db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await db.SaveChangesAsync();
        }
        #endregion

        #region Auth0 API - Helpers
        public async Task<ManagementApiClient> GetClientAsync()
        {
            var token = await GetTokenAsync();
            return new ManagementApiClient(token.Token, new Uri($"https://{Engine.Auth0Configuration.Domain}/api/v2"));
        }
        private async Task<AuthToken> GetTokenAsync()
        {
            var client = new RestClient($"https://{Engine.Auth0Configuration.Domain}/oauth/token");
            var request = new RestRequest(Method.POST);
            client.Timeout = -1;
            request.AddHeader("Content-Type", "application/json");
            var requestTokenParam = new Auth0TokenRequestParameter()
            {
                ClientId = Engine.Auth0Configuration.ApiClient,
                ClientSecret = Engine.Auth0Configuration.ApiSecret,
                Audience = $"https://{Engine.Auth0Configuration.Domain}/api/v2/",
                GrantType = "client_credentials"
            };
            request.AddParameter("application/json", requestTokenParam.ToJson(), ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            return JsonConvert.DeserializeObject<AuthToken>(response.Content);
        }
        #endregion

        #region Auth0 API - Users
        public async Task<System.Collections.Generic.IPagedList<Auth0User>> GetUsers(string search = "", int page = 0, int pageSize = 50)
        {
            var client = await GetClientAsync();
            var request = new GetUsersRequest()
            {
                Query = search
            };
            var users = await client.Users.GetAllAsync(request, new PaginationInfo(page, pageSize, true));
            var pagedList = new System.Collections.Generic.PagedList<Auth0User>()
            {
                List = users.Select(u => new Auth0User(u)).ToList(),
                TotalCount = users.Paging.Total,
                Search = search,
                PageIndex = page,
                PageSize = pageSize,
                TotalPages = pageSize == 0 ? 0 : (int)(users.Paging.Total / pageSize) + 1
            };
            return pagedList;
        }
        public async Task<Auth0User> GetUserById(string userId)
        {
            var client = await GetClientAsync();
            var user = await client.Users.GetAsync(userId);
            return new Auth0User(user);
        }
        public async Task<System.Collections.Generic.IList<Auth0User>> GetUserByEmail(string email)
        {
            if (!email.IsSet())
            {
                return null;
            }
            var client = await GetClientAsync();
            var users = await client.Users.GetUsersByEmailAsync(email);
            return users.Select(u => new Auth0User(u)).ToList();
        }
        public async Task<Auth0User> CreateUserWithPassword(ApplicationUser user, string password)
        {
            var client = await GetClientAsync();
            var newUser = await client.Users.CreateAsync(new UserCreateRequest()
            {
                Connection = Constants.UsernamePasswordConnectionName,
                Email = user.Email,
                Password = password,
                PhoneNumber = user.PhoneNumber,
                VerifyEmail = false
            });
            return new Auth0User(newUser);
        }
        public async Task DeleteUser(string userId)
        {
            var client = await GetClientAsync();
            await client.Users.DeleteAsync(userId);
        }
        #endregion

        #region Auth0 API - Roles
        public async Task<Auth0.ManagementApi.Paging.IPagedList<Role>> GetRolesAsync(string search = "", int page = 0, int pageSize = 50)
        {
            var client = await GetClientAsync();
            var request = new GetRolesRequest()
            {
                NameFilter = search
            };
            return await client.Roles.GetAllAsync(request, new PaginationInfo(page, pageSize, true));
        }
        public async Task<Role> GetRoleAsync(string id)
        {
            var cacheKey = $"{Constants.Auth0RoleCacheName}.Id.{id}";
            if (_cache.TryGetValue(cacheKey, out Role cachedObject))
            {
                return cachedObject;
            }
            var client = await GetClientAsync();
            cachedObject = await client.Roles.GetAsync(id);
            if (cachedObject != null)
            {
                _cache.Add(cacheKey, cachedObject);
            }
            return cachedObject;
        }
        public async Task<Role> GetRoleByNameAsync(string name)
        {
            var cacheKey = $"{Constants.Auth0RoleCacheName}.Name.{name}";
            if (_cache.TryGetValue(cacheKey, out Role cachedObject))
            {
                return cachedObject;
            }
            // loop through any pages of search results and find our role.
            int counter = 0;
            var roles = await GetRolesAsync(name, counter);
            while (cachedObject == null || (roles.Paging.Start + roles.Paging.Length) < roles.Paging.Total)
            {
                cachedObject = roles.SingleOrDefault(r => r.Name.ToUpperInvariant() == name.ToUpperInvariant());
                if (cachedObject != null)
                {
                    _cache.Add(cacheKey, cachedObject);
                }
                roles = await GetRolesAsync(name, counter++);
            }
            return cachedObject;
        }
        public async Task<Role> CreateRoleAsync(ApplicationRole internalRole)
        {
            var client = await GetClientAsync();
            Role newRole = null;
            if (internalRole.RemoteId.IsSet())
            {
                newRole = await GetRoleAsync(internalRole.RemoteId);
                if (newRole != null)
                {
                    return newRole;
                }
            }
            try
            {
                newRole = await client.Roles.CreateAsync(new RoleCreateRequest()
                {
                    Name = internalRole.Name,
                    Description = internalRole.Id
                });
            }
            catch (Auth0.Core.Exceptions.ErrorApiException ex)
            {
                // already exists - try get by name.
                if (ex.ApiError.Error == "Conflict")
                {
                    newRole = await GetRoleByNameAsync(internalRole.Name);
                }
            }

            return newRole;
        }
        public async Task<bool> RoleExistsAsync(string id)
        {
            return await GetRoleAsync(id) != null;
        }
        public async Task<Role> UpdateRoleAsync(string id, string newName)
        {
            var remoteRole = await GetRoleAsync(id);
            if (remoteRole != null)
            {
                var client = await GetClientAsync();
                remoteRole = await client.Roles.UpdateAsync(id, new RoleUpdateRequest()
                {
                    Name = newName
                });
            }
            return remoteRole;
        }
        internal async Task DeleteRole(string role)
        {
            var client = await GetClientAsync();
            var remoteRole = await GetRoleAsync(role);
            if (remoteRole != null)
            {
                await client.Roles.DeleteAsync(remoteRole.Id);
            }
        }
        public async Task AddUserToRolesAsync(ApplicationUser user, ApplicationRole[] roles)
        {
            var client = await GetClientAsync();
            if (user.ConnectedAuth0Accounts != null)
            {
                foreach (var account in user.ConnectedAuth0Accounts)
                {
                    string[] roleIds = roles.Select(r => r.RemoteId).ToArray();
                    await client.Users.AssignRolesAsync(account.UserId, new Auth0.ManagementApi.Models.AssignRolesRequest
                    {
                        Roles = roleIds
                    });
                }
            }
        }
        public async Task RemoveUserFromRolesAsync(ApplicationUser user, ApplicationRole[] roles)
        {
            var client = await GetClientAsync();
            if (user.ConnectedAuth0Accounts != null)
            {
                foreach (var account in user.ConnectedAuth0Accounts)
                {
                    string[] roleIds = roles.Select(r => r.RemoteId).ToArray();
                    await client.Users.RemoveRolesAsync(account.UserId, new Auth0.ManagementApi.Models.AssignRolesRequest
                    {
                        Roles = roleIds
                    });
                }
            }
        }
        #endregion

        #region Auth0 API - Email Validation
        public async Task<Ticket> GetEmailVerificationTicket(string accountId, string returnUrl)
        {
            var client = await GetClientAsync();
            var ticket = await client.Tickets.CreateEmailVerificationTicketAsync(new EmailVerificationTicketRequest()
            {
                ResultUrl = returnUrl,
                UserId = accountId
            });
            return ticket;
        }
        #endregion

    }
}