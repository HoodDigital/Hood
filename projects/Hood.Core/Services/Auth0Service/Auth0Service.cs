using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Hood.Caching;
using Hood.Contexts;
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
    public class Auth0Service : IAuth0Service
    {
        protected IHoodCache _cache { get; set; }
        public Auth0Service()
        {
            _cache = Engine.Services.Resolve<IHoodCache>();
        }

        #region Client

        protected async Task<ManagementApiClient> GetClientAsync()
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

        #region Users

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

        public async Task<User> UpdateUser(string userId, UserUpdateRequest updateRequest)
        {
            var client = await GetClientAsync();
            var user = await client.Users.UpdateAsync(userId, updateRequest);
            return user;
        }

        public async Task<System.Collections.Generic.IList<User>> GetUsersByEmail(string email)
        {
            if (!email.IsSet())
            {
                return null;
            }
            var client = await GetClientAsync();
            var users = await client.Users.GetUsersByEmailAsync(email);
            return users.ToList();
        }

        public async Task<User> CreateUserWithPassword(Auth0User user, string password)
        {
            var client = await GetClientAsync();
            var newUser = await client.Users.CreateAsync(new UserCreateRequest()
            {
                Connection = Authentication.UsernamePasswordConnectionName,
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

        #region Account Linking

        public async Task LinkAccountAsync(Auth0Identity primaryAccount, string fullAuthUserId)
        {
            var authProviderName = fullAuthUserId.Split('|')[0];
            var authUserId = fullAuthUserId.Split('|')[1];
            var client = await GetClientAsync();
            var response = await client.Users.LinkAccountAsync(primaryAccount.Id, new Auth0.ManagementApi.Models.UserAccountLinkRequest()
            {
                Provider = authProviderName,
                UserId = authUserId
            });
        }

        public async Task UnlinkAccountAsync(Auth0Identity primaryAccount, Auth0Identity identityToDisconnect)
        {
            var client = await GetClientAsync();
            var response = await client.Users.UnlinkAccountAsync(primaryAccount.Id, identityToDisconnect.Provider, identityToDisconnect.LocalUserId);
        }

        public async Task<Ticket> CreatePasswordChangeTicket(Auth0Identity remoteUser)
        {
            var client = await GetClientAsync();
            return await client.Tickets.CreatePasswordChangeTicketAsync(new Auth0.ManagementApi.Models.PasswordChangeTicketRequest()
            {
                UserId = remoteUser.LocalUserId
            });
        }

        #endregion

        #region Email Validation

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

        #region Roles 

        public async Task<System.Collections.Generic.IPagedList<Role>> GetRoles(string search = "", int page = 0, int pageSize = 50)
        {
            var client = await GetClientAsync();
            var request = new GetRolesRequest()
            {
                NameFilter = search
            };
            var roles = await client.Roles.GetAllAsync(request, new PaginationInfo(page, pageSize, true));
            var pagedList = new System.Collections.Generic.PagedList<Role>()
            {
                List = roles.ToList(),
                TotalCount = roles.Paging.Total,
                Search = search,
                PageIndex = page,
                PageSize = pageSize,
                TotalPages = pageSize == 0 ? 0 : (int)(roles.Paging.Total / pageSize) + 1
            };
            return pagedList;
        }

        public async Task<Role> GetRoleById(string roleId)
        {
            var client = await GetClientAsync();
            var Role = await client.Roles.GetAsync(roleId);
            return Role;
        }

        public async Task<Role> GetRoleByName(string roleName)
        {
            var roles = await GetRoles(roleName);
            if (roles.TotalCount > 0)
            {
                return roles.List.FirstOrDefault();
            }
            return null;
        }

        public async Task DeleteRole(string roleId)
        {
            var client = await GetClientAsync();
            await client.Roles.DeleteAsync(roleId);
        }

        public async Task<System.Collections.Generic.PagedList<Role>> GetRolesForUser(string userId, int page = 0, int pageSize = 50)
        {
            var client = await GetClientAsync();
            var roles = await client.Users.GetRolesAsync(userId, new PaginationInfo(page, pageSize, true));
            var pagedList = new System.Collections.Generic.PagedList<Role>()
            {
                List = roles.ToList(),
                TotalCount = roles.Paging.Total,
                Search = "",
                PageIndex = page,
                PageSize = pageSize,
                TotalPages = pageSize == 0 ? 0 : (int)(roles.Paging.Total / pageSize) + 1
            };
            return pagedList;
        }

        public async Task AssignRolesToUser(string userId, string[] roleIds)
        {
            var client = await GetClientAsync();
            await client.Users.AssignRolesAsync(userId, new AssignRolesRequest()
            {
                Roles = roleIds
            });
        }

        public async Task RemoveRolesFromUser(string userId, string[] roleIds)
        {
            var client = await GetClientAsync();
            await client.Users.RemoveRolesAsync(userId, new AssignRolesRequest()
            {
                Roles = roleIds
            });
        }

        public async Task<Role> CreateRoleForLocalRole(Auth0Role role)
        {
            var client = await GetClientAsync();
            var remoteRole = await client.Roles.CreateAsync(new RoleCreateRequest() {
                Name = role.Name,
                Description = $"Role connected to role {role.Id} on Hood CMS."
            });
            return remoteRole;
        }

        #endregion

    }
}