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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

namespace Hood.Services
{
    public class Auth0Service
    {
        protected IHoodCache _cache { get; set; }
        protected HoodDbContext _db { get; set; }
        public Auth0Service()
        {
            _cache = Engine.Services.Resolve<IHoodCache>();
            _db = Engine.Services.Resolve<HoodDbContext>();
        }

        #region Data CRUD
        public async Task<ApplicationUser> GetUserByAuth0UserId(string userId)
        {
            var auth0user = await _db.Auth0Users.Include(au => au.User).SingleOrDefaultAsync(au => au.UserId == userId);
            if (auth0user != null)
            {
                return auth0user.User;
            }
            return null;
        }
        public async Task<ApplicationUser> GetUserByAuth0Id(string id)
        {
            var auth0user = await _db.Auth0Users.Include(au => au.User).SingleOrDefaultAsync(au => au.Id == id);
            if (auth0user != null)
            {
                return auth0user.User;
            }
            return null;
        }
        public async Task GetLocalAuthIdentity(string userId)
        {
            var db = Engine.Services.Resolve<HoodDbContext>();
            var userToRemove = db.Auth0Users.SingleOrDefault(u => u.UserId == userId);
            db.Entry(userToRemove).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        public async Task<Auth0Identity> CreateLocalAuthIdentity(string fullAuthUserId, ApplicationUser user, string picture)
        {
            var authProviderName = fullAuthUserId.Split('|')[0];
            var authUserId = fullAuthUserId.Split('|')[1];
            User newAuthUser = null;
            try
            {
                newAuthUser = await GetUserById(fullAuthUserId);
            }
            catch (Auth0.Core.Exceptions.ErrorApiException)
            {
            }

            var primaryIdentity = user.GetPrimaryIdentity();
            if (newAuthUser == null && primaryIdentity != null)
            {
                newAuthUser = await GetUserById(primaryIdentity.Id);
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

            var db = Engine.Services.Resolve<HoodDbContext>();
            db.Add(newIdentity);
            await db.SaveChangesAsync();
            return newIdentity;
        }
        public async Task DeleteLocalAuthIdentity(string id)
        {
            var db = Engine.Services.Resolve<HoodDbContext>();
            var userToRemove = db.Auth0Users.SingleOrDefault(u => u.Id == id);
            db.Entry(userToRemove).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        public async Task UpdateLocalAuthIdentity(Auth0Identity user)
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
            var cacheKey = $"{typeof(AuthToken)}";
            if (_cache.TryGetValue(cacheKey, out AuthToken cachedObject))
            {
                return cachedObject;
            }

            var client = new RestClient($"https://{Engine.Auth0Configuration.Domain}/oauth/token");
            var request = new RestRequest(Method.POST);
            client.Timeout = -1;
            request.AddHeader("Content-Type", "application/json");
            var requestTokenParam = new Auth0TokenRequestParameter()
            {
                ClientId = Engine.Auth0Configuration.ClientId,
                ClientSecret = Engine.Auth0Configuration.ClientSecret,
                Audience = $"https://{Engine.Auth0Configuration.Domain}/api/v2/",
                GrantType = "client_credentials"
            };
            request.AddParameter("application/json", requestTokenParam.ToJson(), ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            cachedObject = JsonConvert.DeserializeObject<AuthToken>(response.Content);

            if (cachedObject != null)
            {
                _cache.Add(cacheKey, cachedObject, new TimeSpan(0, 5, 0));
            }
            return cachedObject;
        }
        #endregion

        #region Auth0 API - Users
        public async Task<System.Collections.Generic.IPagedList<User>> GetUsers(string search = "", int page = 0, int pageSize = 50)
        {
            var client = await GetClientAsync();
            var request = new GetUsersRequest()
            {
                Query = search
            };
            var users = await client.Users.GetAllAsync(request, new PaginationInfo(page, pageSize, true));
            var pagedList = new System.Collections.Generic.PagedList<User>()
            {
                List = users.ToList(),
                TotalCount = users.Paging.Total,
                Search = search,
                PageIndex = page,
                PageSize = pageSize,
                TotalPages = pageSize == 0 ? 0 : (int)(users.Paging.Total / pageSize) + 1
            };
            return pagedList;
        }
        public async Task<User> GetUserById(string userId)
        {
            var client = await GetClientAsync();
            var user = await client.Users.GetAsync(userId);
            return user;
        }
        public async Task<System.Collections.Generic.IList<User>> GetUserByEmail(string email)
        {
            if (!email.IsSet())
            {
                return null;
            }
            var client = await GetClientAsync();
            var users = await client.Users.GetUsersByEmailAsync(email);
            return users.ToList();
        }
        public async Task<User> CreateUserWithPassword(ApplicationUser user, string password)
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
            return newUser;
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

        public async Task InitialiseApp(string[] additionalLogoutUrls = null, string[] additionalCallbacks = null, string[] additionalOrigins = null, string[] additionalWebOrigins = null)
        {
            ApplicationUser siteAdmin = await Engine.AccountManager.GetUserByEmailAsync(Engine.SiteOwnerEmail);
            var allRoles = await Engine.AccountManager.GetRolesAsync();
            var requiredRoles = allRoles.List.Where(a => Models.Roles.All.Contains(a.Name));
            var extraRoles = allRoles.List.Where(a => !Models.Roles.All.Contains(a.Name));
            foreach (ApplicationRole extraLocalRole in extraRoles)
            {
                // Ensure it has a remote id linked to it.
                if (!extraLocalRole.RemoteId.IsSet())
                {
                    await Engine.AccountManager.CreateRoleAsync(extraLocalRole.Name);
                }
            }
            // Make sure site admin is in all required roles.
            await Engine.AccountManager.AddUserToRolesAsync(siteAdmin, requiredRoles.ToArray());
            if (Engine.Auth0Enabled)
            {
                await AddUserToRolesAsync(siteAdmin, requiredRoles.ToArray());
            }

            // Check the rule is set to provide roles from Auth0
            var client = await GetClientAsync();
            var rules = await client.Rules.GetAllAsync(new Auth0.ManagementApi.Models.GetRulesRequest()
            {
                Stage = "login_success"
            });

            var roleScript = @"
function (user, context, callback) {
    const role_namespace = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    if (context.authorization !== null) {
        if (context.authorization.roles !== null) {
            context.idToken[role_namespace] = context.authorization.roles;
        } else {
            console.log('context.authorization.roles is null');
        }
    } else {
        console.log('context.authorization is null');
    }
    return callback(null, user, context);
}";
            var roleRule = rules.SingleOrDefault(r => r.Name == Hood.Identity.Constants.AddRoleClaimsRuleName);
            if (roleRule == null)
            {
                await client.Rules.CreateAsync(new Auth0.ManagementApi.Models.RuleCreateRequest()
                {
                    Script = roleScript,
                    Name = Hood.Identity.Constants.AddRoleClaimsRuleName,
                    Enabled = true,
                    Stage = "login_success"
                });
            }
            else
            {
                await client.Rules.UpdateAsync(roleRule.Id, new Auth0.ManagementApi.Models.RuleUpdateRequest()
                {
                    Script = roleScript,
                    Enabled = true
                });
            }

            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
            var contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
            var appClient = await client.Clients.GetAsync(Engine.Auth0Configuration.ClientId);
            var context = contextAccessor.HttpContext;

            var allowedLogoutUrls = (appClient.AllowedLogoutUrls == null || appClient.AllowedLogoutUrls.Count() == 0) ? new HashSet<string>() : appClient.AllowedLogoutUrls.ToHashSet();
            var allowedOrigins = (appClient.AllowedOrigins == null || appClient.AllowedOrigins.Count() == 0) ? new HashSet<string>() : appClient.AllowedOrigins.ToHashSet();
            var callbacks = (appClient.Callbacks == null || appClient.Callbacks.Count() == 0) ? new HashSet<string>() : appClient.Callbacks.ToHashSet();
            var webOrigins = (appClient.WebOrigins == null || appClient.WebOrigins.Count() == 0) ? new HashSet<string>() : appClient.WebOrigins.ToHashSet();

            allowedLogoutUrls.Add(linkGenerator.GetUriByAction(contextAccessor.HttpContext, "Index", "Home"));
            allowedLogoutUrls.Add(linkGenerator.GetUriByAction(contextAccessor.HttpContext, "RemoteSigninFailed", "Account"));
            allowedLogoutUrls.Add(linkGenerator.GetUriByAction(contextAccessor.HttpContext, "Deleted", "Account"));
            if (additionalLogoutUrls != null)
            {
                additionalLogoutUrls.ForEach(a => { allowedLogoutUrls.Add(a); });
            }

            callbacks.Add(linkGenerator.GetUriByAction(contextAccessor.HttpContext, "Index", "Home"));
            callbacks.Add($"{context.Request.Scheme}://{context.Request.Host}/callback");
            if (additionalCallbacks != null)
            {
                additionalCallbacks.ForEach(a => { callbacks.Add(a); });
            }

            allowedOrigins.Add($"{context.Request.Scheme}://{context.Request.Host}");
            if (additionalOrigins != null)
            {
                additionalOrigins.ForEach(a => { allowedOrigins.Add(a); });
            }

            webOrigins.Add($"{context.Request.Scheme}://{context.Request.Host}");
            if (additionalWebOrigins != null)
            {
                additionalWebOrigins.ForEach(a => { webOrigins.Add(a); });
            }

            await client.Clients.UpdateAsync(Engine.Auth0Configuration.ClientId, new ClientUpdateRequest() { 
                AllowedLogoutUrls = allowedLogoutUrls.ToArray(), 
                AllowedOrigins = allowedOrigins.ToArray(), 
                Callbacks = callbacks.ToArray(), 
                WebOrigins = webOrigins.ToArray()
            });
        }

        public async Task<Auth0.ManagementApi.Paging.IPagedList<Role>> GetRolesByUserAsync(string userId, int page = 0, int pageSize = 50)
        {
            var client = await GetClientAsync();
            return await client.Users.GetRolesAsync(userId, new PaginationInfo(page, pageSize, true));
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
                var account = user.GetPrimaryIdentity();
                if (account != null)
                {
                    string[] roleIds = roles.Select(r => r.RemoteId).ToArray();
                    await client.Users.AssignRolesAsync(account.Id, new Auth0.ManagementApi.Models.AssignRolesRequest
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
                var account = user.GetPrimaryIdentity();
                if (account != null)
                {
                    string[] roleIds = roles.Select(r => r.RemoteId).ToArray();
                    await client.Users.RemoveRolesAsync(account.Id, new Auth0.ManagementApi.Models.AssignRolesRequest
                    {
                        Roles = roleIds
                    });
                }
            }
        }
        public async Task SyncLocalRoles(ApplicationUser user, List<string> remoteRoles)
        {
            var accountRepository = Engine.Services.Resolve<IAccountRepository>();
            var localRoles = await accountRepository.GetRolesForUser(user);
            if (!localRoles.Select(r => r.Name).All(remoteRoles.Contains) || localRoles.Count() != remoteRoles.Count)
            {
                // remote roles are out of sync... re-sync. 
                var extraLocalRoles = new List<ApplicationRole>();
                foreach (var role in localRoles)
                {
                    if (!remoteRoles.Contains(role.Name))
                    {
                        extraLocalRoles.Add(await accountRepository.GetRoleAsync(role.Name));
                    }
                }

                var extraRemoteRoles = new List<ApplicationRole>();
                foreach (var role in remoteRoles)
                {
                    if (!localRoles.Any(r => r.NormalizedName == role.ToUpperInvariant()))
                    {
                        extraRemoteRoles.Add(await accountRepository.GetRoleAsync(role));
                    }
                }

                // Add any remote roles that are missing from the local user.
                if (extraRemoteRoles.Count > 0)
                {
                    await accountRepository.AddUserToRolesAsync(user, extraRemoteRoles.ToArray());
                }
                // Remove any local roles that are on the local user that are not on the remote.
                if (extraLocalRoles.Count > 0)
                {
                    await accountRepository.RemoveUserFromRolesAsync(user, extraLocalRoles.ToArray());
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