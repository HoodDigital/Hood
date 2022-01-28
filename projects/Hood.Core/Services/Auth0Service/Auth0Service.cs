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
                Connection = "Username-Password-Authentication",
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
        public async Task<Role> GetRoleAsync(string role)
        {
            var roles = await GetRoles(role, 0, 100);
            if (roles.List != null)
            {
                return roles.List.SingleOrDefault(r => r.Name.ToUpperInvariant() == role.ToUpperInvariant());
            }
            return null;
        }
        public async Task<Role> GetOrCreateRoleAsync(string role)
        {
            var newRole = await GetRoleAsync(role);
            if (newRole == null)
            {
                var client = await GetClientAsync();
                newRole = await client.Roles.CreateAsync(new RoleCreateRequest()
                {
                    Name = role,
                    Description = role
                });
            }
            return newRole;
        }
        public async Task<bool> RoleExistsAsync(string role)
        {
            return await GetRoleAsync(role) != null;
        }
        public async Task UpdateRole(string role, string newName)
        {
            var client = await GetClientAsync();
            var remoteRole = await GetRoleAsync(role);
            if (remoteRole != null)
            {
                remoteRole = await client.Roles.UpdateAsync(remoteRole.Id, new RoleUpdateRequest()
                {
                    Name = newName
                });
            }
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
        public async Task<System.Collections.Generic.IPagedList<Role>> GetRoles(string search, int page = 0, int pageSize = 50)
        {
            var client = await GetClientAsync();
            var roles = await client.Roles.GetAllAsync(new GetRolesRequest()
            {
                NameFilter = search
            }, new PaginationInfo(page, pageSize, true));

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