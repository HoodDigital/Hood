using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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

        public async Task<ApplicationUser> OnTicketReceived(TicketReceivedContext e)
        {
            // check if user exists - if so, and signups are allowed via remote, redirect to complete profile page.
            var repo = Engine.Services.Resolve<IAccountRepository>();
            var principal = e.Principal;
            var userId = e.Principal.GetUserId();
            var user = await repo.GetUserByAuth0Id(userId);
            if (user != null)
            {
                // user exists and has auth0 account linked to it.
                return user;
            }
            // check if the user has an account, via email                           
            user = await repo.GetUserByEmailAsync(e.Principal.Identity.Name);
            if (user != null)
            {
                // user exists, but the current Auth0 signin method is not saved, force them into the connect account flow.
                var identity = (ClaimsIdentity)principal.Identity;
                identity.AddClaim(new Claim(HoodClaimTypes.AccountNotConnected, "true"));
                await e.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, e.Properties);
                return user;
            }

            if (Engine.Settings.Account.AllowRemoteSignups)
            {
                // user does not exist, create and send them to the complete signup page.
                var authService = new Auth0Service();
                var authUser = await authService.GetUserById(userId);
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

                var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
                var returnUrl = linkGenerator.GetPathByAction("Index", "Manage", new { r = "new-account-connection" });
                e.Response.Redirect(returnUrl);
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

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response;
            }
            else
            {
                throw new ApplicationException($"Remote Auth0 service returned a {response.StatusCode.ToString().CamelCaseToString().ToLower()} status from https://{Engine.Auth0Configuration.Domain}/{path}.");
            }
        }

        public async Task<List<Auth0Role>> GetRoles(string userId = null)
        {
            var response = await CallApi("api/v2/roles");
            return JsonConvert.DeserializeObject<List<Auth0Role>>(response.Content);
        }

        public async Task<PagedList<Auth0User>> GetUsers(string search = "", int page = 0, int pageSize = 50)
        {
            var response = await CallApi("api/v2/users?include_totals=true", parameters: new List<Parameter>
            {
                new Parameter("page", page, ParameterType.QueryString),
                new Parameter("per_page", pageSize, ParameterType.QueryString),
                new Parameter("q", search, ParameterType.QueryString)
            });
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

        public async Task<Auth0User> GetUserById(string userId)
        {
            var response = await CallApi($"api/v2/users/{userId}");
            var user = JsonConvert.DeserializeObject<Auth0User>(response.Content);
            return user;
        }

        public async Task<Auth0User> GetUserByEmail(string email = "")
        {
            var response = await CallApi("api/v2/users-by-email", parameters: new List<Parameter>
            {
                new Parameter("email", email.ToLower(), ParameterType.QueryString)
            });
            var users = JsonConvert.DeserializeObject<List<Auth0User>>(response.Content);
            if (users.Count == 1)
            {
                return users.SingleOrDefault();
            }

            return null;
        }
    }
}