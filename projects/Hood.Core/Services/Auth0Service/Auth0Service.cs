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
        protected Auth0IdentityContext _db { get; set; }
        public Auth0Service()
        {
            _cache = Engine.Services.Resolve<IHoodCache>();
            _db = Engine.Services.Resolve<Auth0IdentityContext>();
        }

        #region Data CRUD
        public async Task<Auth0User> GetUserByAuth0UserId(string userId)
        {
            var auth0user = await _db.Auth0Users.Include(au => au.User).SingleOrDefaultAsync(au => au.UserId == userId);
            if (auth0user != null)
            {
                return auth0user.User;
            }
            return null;
        }
        public async Task<Auth0User> GetUserByAuth0Id(string id)
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
            var db = Engine.Services.Resolve<Auth0IdentityContext>();
            var userToRemove = db.Auth0Users.SingleOrDefault(u => u.UserId == userId);
            db.Entry(userToRemove).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        public async Task<Auth0Identity> CreateLocalAuthIdentity(string fullAuthUserId, Auth0User user, string picture)
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
            newIdentity.UserId = user.Id;
            newIdentity.Picture = picture;

            var db = Engine.Services.Resolve<Auth0IdentityContext>();
            db.Add(newIdentity);
            await db.SaveChangesAsync();
            return newIdentity;
        }
        public async Task DeleteLocalAuthIdentity(string id)
        {
            var db = Engine.Services.Resolve<Auth0IdentityContext>();
            var userToRemove = db.Auth0Users.SingleOrDefault(u => u.Id == id);
            db.Entry(userToRemove).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        public async Task UpdateLocalAuthIdentity(Auth0Identity user)
        {
            var db = Engine.Services.Resolve<Auth0IdentityContext>();
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
        public async Task<User> CreateUserWithPassword(Auth0User user, string password)
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