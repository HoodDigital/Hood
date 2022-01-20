using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using RestSharp;

namespace Hood.Services
{
    public class AuthToken
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }
        [JsonProperty("token_type")]
        public string Type { get; set; }
        public string ToAuthHeader() { return $"{Type} {Token}"; }
    }

    public class Auth0TokenRequestParameter
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
        [JsonProperty("audience")]
        public string Audience { get; set; }
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }
    }

    public class Auth0Service
    {
        public static Claim RequiresSetupClaim = new Claim("account-setup", "account-setup");
        public static bool RequiresSetup(Claim claim)
        {
            if (claim == RequiresSetupClaim)
            {
                return true;
            }
            return false;
        }

        public async Task<ApplicationUser> OnTicketReceived(TicketReceivedContext e)
        {
            // check if user exists - if so, and signups are allowed via remote, redirect to complete profile page.
            var repo = Engine.Services.Resolve<IAccountRepository>();
            var principal = e.Principal;

            var user = await repo.GetUserByAuth0Id(e.Principal.GetUserId());
            if (user != null)
            {
                // user exists and has auth0 account linked to it.
                return user;
            }
            // check if the user has an account, via email                           
            user = await repo.GetUserByEmailAsync(e.Principal.Identity.Name);
            if (user != null)
            {
                // user exists, but the current Auth0 signin method is not saved.
                await CreateAuth0User(e.Principal.GetUserId(), user);
                return user;
            }

            if (Engine.Settings.Account.AllowRemoteSignups)
            {
                // user does not exist, create and send them to the complete signup page.
                var authService = new Auth0Service();
                var authUser = await authService.GetUserById(e.Principal.GetUserId());
                if (authUser == null)
                {
                    throw new ApplicationException("Something went wrong while authorizing your account.");
                }
                user = new ApplicationUser
                {
                    UserName = authUser.UserName.IsSet() ? authUser.UserName : authUser.Email,
                    Email = authUser.Email,
                    FirstName = authUser.FirstName,
                    LastName = authUser.LastName,
                    DisplayName = authUser.DisplayName,
                    PhoneNumber = authUser.PhoneNumber,
                    CreatedOn = DateTime.UtcNow,
                    LastLogOn = DateTime.UtcNow,
                    LastLoginLocation = authUser.LastLoginIp,
                    LastLoginIP = authUser.LastLoginIp
                };
                await repo.CreateAsync(user, null);

                await CreateAuth0User(e.Principal.GetUserId(), user);

                //set the new principal, adding the claim to mark required setup.
                var identity = (ClaimsIdentity)principal.Identity;
                identity.AddClaim(RequiresSetupClaim);
                await e.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, e.Properties);

                e.Response.Redirect("/account/auth/complete-signup");
                e.HandleResponse();

            }
            
            return user;
        }

        private async Task<Auth0User> CreateAuth0User(string authUserId, ApplicationUser user)
        {
            var newAuthUser = await GetUserById(authUserId);
            var db = Engine.Services.Resolve<HoodDbContext>();
            newAuthUser.UserId = user.Id;
            db.Add(newAuthUser);
            await db.SaveChangesAsync();
            return newAuthUser;
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

        private async Task<IRestResponse> CallApi(string path, Method method = Method.GET, object body = null, List<Parameter> parameters = null)
        {
            var client = new RestClient($"https://{Engine.Auth0Configuration.Domain}/");
            var request = new RestRequest(path, method);
            var token = await GetTokenAsync();
            request.AddHeader("authorization", token.ToAuthHeader());
            if (body != null)
            {
                request.AddJsonBody(body);
            }
            if (parameters != null)
            {
                parameters.ForEach(p => request.AddParameter(p));
            }
            var response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<List<Auth0Role>> GetRoles(string userId = null)
        {
            var response = await CallApi("api/v2/roles");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<List<Auth0Role>>(response.Content);
            }
            return null;
        }

        public async Task<PagedList<Auth0User>> GetUsers(string search = "", int page = 0, int pageSize = 50)
        {
            var response = await CallApi("api/v2/users?include_totals=true", parameters: new List<Parameter>
            {
                new Parameter("page", page, ParameterType.QueryString),
                new Parameter("per_page", pageSize, ParameterType.QueryString),
                new Parameter("q", search, ParameterType.QueryString)
            });
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var userList = JsonConvert.DeserializeObject<Auth0UserList>(response.Content);
                var pagedList = new PagedList<Auth0User>()
                {
                    List = userList.Users,
                    TotalCount = userList.Total,
                    Search = search,
                    PageIndex = page,
                    PageSize = pageSize,
                    TotalPages = pageSize == 0 ? 0 : (int)(userList.Total / pageSize) + 1
                };
                return pagedList;
            }
            return new PagedList<Auth0User>();
        }

        public async Task<Auth0User> GetUserById(string userId)
        {
            var response = await CallApi($"api/v2/users/{userId}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var user = JsonConvert.DeserializeObject<Auth0User>(response.Content);
                return user;
            }
            return null;
        }

        public async Task<Auth0User> GetUserByEmail(string email = "")
        {
            var response = await CallApi("api/v2/users-by-email", parameters: new List<Parameter>
            {
                new Parameter("email", email.ToLower(), ParameterType.QueryString)
            });
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var users = JsonConvert.DeserializeObject<List<Auth0User>>(response.Content);
                if (users.Count == 1)
                {
                    return users.SingleOrDefault();
                }
            }
            return null;
        }
    }
}