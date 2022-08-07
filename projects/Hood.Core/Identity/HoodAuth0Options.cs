using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Routing;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Hood.Identity
{
    public class HoodAuth0Options : IHoodAuth0Options
    {
        public HoodAuth0Options(IConfiguration config)
        {
            this.Domain = this.Domain.IsSet() ? this.Domain : config["Identity:Auth0:Domain"];
            this.ClientId = this.ClientId.IsSet() ? this.ClientId : config["Identity:Auth0:ClientId"];
            this.Scope = this.Scope.IsSet() ? this.Scope : "openid profile email";
        }

#nullable enable

        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string Scope { get; set; }
        public string? CallbackPath { get; set; }
        public string? Organization { get; set; }
        public IDictionary<string, string>? LoginParameters { get; set; }
        public OpenIdConnectEvents? OpenIdConnectEvents { get; set; }
        public string? ResponseType { get; set; }
        public HttpClient? Backchannel { get; set; }
        public TimeSpan? MaxAge { get; set; }
        public bool ForwardToReturnUriOnSignup { get; set; } = false;
        public string AccountControllerName { get; set; } = "Account";
        public string SignupCompleteAction { get; set; } = "Index";
        public string SignupCompleteController { get; set; } = "Manage";


#nullable disable

        public OpenIdConnectEvents AsOpenIdConnectEvents()
        {
            return new OpenIdConnectEvents()
            {
                OnTokenResponseReceived = this.OnTokenResponseReceived,
                OnTicketReceived = this.OnTicketReceived,
                OnRemoteFailure = this.OnRemoteFailure,
                OnRemoteSignOut = this.OnRemoteSignOut,
                OnSignedOutCallbackRedirect = this.OnSignedOutCallbackRedirect,
                OnRedirectToIdentityProviderForSignOut = this.OnRedirectToIdentityProviderForSignOut,
                OnAccessDenied = this.OnAccessDenied,
                OnMessageReceived = this.OnMessageReceived,
                OnAuthorizationCodeReceived = this.OnAuthorizationCodeReceived,
                OnAuthenticationFailed = this.OnAuthenticationFailed,
                OnTokenValidated = this.OnTokenValidated,
                OnUserInformationReceived = this.OnUserInformationReceived
            };
        }

        public virtual Task OnTokenResponseReceived(TokenResponseReceivedContext e) { return Task.CompletedTask; }
        public virtual async Task OnTicketReceived(TicketReceivedContext e)
        {
            var authService = new Auth0Service();
            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
            var repo = Engine.Services.Resolve<IAuth0AccountRepository>();
            var principal = e.Principal;
            var userId = e.Principal.GetUserId();
            var returnUrl = e.ReturnUri;

            var identity = (ClaimsIdentity)principal.Identity;

            // Check if user exists and is linked to this Auth0 account
            var user = await authService.GetUserByAuth0Id(userId);
            if (user != null)
            {
                // user exists and has auth0 account linked to it.
                var emailVerifiedClaim = e.Principal.FindFirst("email_verified");
                if (user.Active || (emailVerifiedClaim != null && emailVerifiedClaim.Value == "true"))
                {
                    identity.AddClaim(new Claim(Identity.HoodClaimTypes.Active, "true"));
                    await e.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, e.Properties);
                }

                await SetDefaultClaims(e, user);
                return;
            }

            // check if the user has an account, via email, if so, the should be asked to link to this account                           
            user = await repo.GetUserByEmailAsync(e.Principal.GetEmail());
            if (user != null)
            {
                // user exists, but the current Auth0 signin method is not saved, force them into the connect account flow.
                if (user.ConnectedAuth0Accounts != null && user.ConnectedAuth0Accounts.Count() > 0)
                {
                    identity.AddClaim(new Claim(Identity.HoodClaimTypes.AccountLinkRequired, "true"));
                }

                // This is a legacy account just send to the local account connect flow - 
                // Email needs to be verified to attach to a local account.

                identity.AddClaim(new Claim(Identity.HoodClaimTypes.AccountNotConnected, "true"));

                await SetDefaultClaims(e, user);

                identity.AddClaim(new Claim(Identity.HoodClaimTypes.AccountCreationFailed, "true"));
                e.Response.Redirect(linkGenerator.GetPathByAction("ConnectAccount", this.AccountControllerName, new { returnUrl = e.ReturnUri }));
                e.HandleResponse();
                return;
            }


            if (!Engine.Settings.Account.AllowRemoteSignups)
            {
                // user has not been found, or created & signups are disabled on this end
                identity.AddClaim(new Claim(Identity.HoodClaimTypes.AccountCreationFailed, "true"));
                returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", this.AccountControllerName, new { r = "signup-disabled" });
                e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", this.AccountControllerName, new { returnUrl }));
                e.HandleResponse();
                return;
            }

            // user does not exist, signups are allowed, so create and send them to the complete signup page.
            var authUser = await authService.GetUserById(userId);
            if (authUser == null)
            {
                throw new ApplicationException("Something went wrong while authorizing your account.");
            }
            user = new Auth0User
            {
                UserName = authUser.UserName.IsSet() ? authUser.UserName : authUser.Email,
                Email = authUser.Email,
                PhoneNumber = authUser.PhoneNumber,
                EmailConfirmed = authUser.EmailVerified.HasValue ? authUser.EmailVerified.Value : false
            };
            var result = await repo.CreateAsync(user);
            if (!result)
            {
                // user has not been found, or created (signups disabled on this end) - signout and forward to failure page.
                identity.AddClaim(new Claim(Identity.HoodClaimTypes.AccountCreationFailed, "true"));
                returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", this.AccountControllerName, new { r = "account-creation-failed" });
                e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", this.AccountControllerName, new { returnUrl }));
                e.HandleResponse();
                return;
            }

            // If the user is still not found, there is a problem... 
            if (user == null)
            {
                // user has not been found, or created (signups disabled on this end) - signout and forward to failure page.
                identity.AddClaim(new Claim(Identity.HoodClaimTypes.AccountCreationFailed, "true"));
                returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", this.AccountControllerName, new { r = "account-linking-failed" });
                e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", this.AccountControllerName, new { returnUrl }));
                e.HandleResponse();
                return;
            }

            // User exists at this point, for sure, but no auth0 connection for it, create it now.
            await authService.CreateLocalAuthIdentity(e.Principal.GetUserId(), user, authUser.Picture);

            if (user.Active)
            {
                // Account is already active, so mark it as such, this allows access to secure areas.

                identity.AddClaim(new Claim(Identity.HoodClaimTypes.Active, "true"));
            }

            await SetDefaultClaims(e, user);

            identity.AddClaim(new Claim(Identity.HoodClaimTypes.AccountCreated, "true"));

            if (ForwardToReturnUriOnSignup)
            {
                e.Response.Redirect(e.ReturnUri);
                e.HandleResponse();
                return;
            }

            // Account setup complete, send user to manage profile with new-account-connection flag.
            returnUrl = linkGenerator.GetPathByAction(this.SignupCompleteAction, this.SignupCompleteController, new { returnUrl = e.ReturnUri, created = true });
            e.Response.Redirect(returnUrl);
            e.HandleResponse();
            return;
        }
        
        public virtual Task OnRemoteFailure(RemoteFailureContext e)
        {
            // // user has not been found, or created (signups disabled on this end) - signout and forward to failure page.
            string reason = "remote-failed";
            string description = "Sign in failed due to a remote error.";
            if (e.Failure == null || e.Failure.Data == null || e.Failure.Data.Count == 0)
            {
                if (e.Failure?.Message != null && e.Failure.Message.ToLower().Contains("message.state"))
                {
                    reason = "state-failure";
                }
            }
            else
            {
                var data = e.Failure.Data.Cast<DictionaryEntry>()
                                            .Where(de => de.Key is string && de.Value is string)
                                            .ToDictionary(de => (string)de.Key, de => (string)de.Value);
                if (data.ContainsKey("error")) { reason = data["error"]; }
                if (data.ContainsKey("error_description")) { description = data["error_description"]; }
            }
            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
            var returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", this.AccountControllerName, new { r = reason, d = description });
            e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", this.AccountControllerName, new { returnUrl, d = description }));
            e.HandleResponse();
            return Task.CompletedTask;
        }
        public virtual Task OnRemoteSignOut(RemoteSignOutContext e) { return Task.CompletedTask; }
        public virtual Task OnSignedOutCallbackRedirect(RemoteSignOutContext e) { return Task.CompletedTask; }
        public virtual Task OnRedirectToIdentityProviderForSignOut(RedirectContext e) { return Task.CompletedTask; }
        public virtual Task OnRedirectToIdentityProvider(RedirectContext e) { return Task.CompletedTask; }
        public virtual Task OnAccessDenied(AccessDeniedContext e) { return Task.CompletedTask; }
        public virtual Task OnMessageReceived(MessageReceivedContext e) { return Task.CompletedTask; }
        public virtual Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext e) { return Task.CompletedTask; }
        public virtual Task OnAuthenticationFailed(AuthenticationFailedContext e)
        {
            // user has not been found, or created (signups disabled on this end) - signout and forward to failure page.
            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
            var returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", this.AccountControllerName, new { r = "auth-failed" });
            e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", this.AccountControllerName, new
            {
                returnUrl
            }));
            e.HandleResponse();
            return Task.CompletedTask;
        }
        public virtual Task OnTokenValidated(TokenValidatedContext e) { return Task.CompletedTask; }
        public virtual Task OnUserInformationReceived(UserInformationReceivedContext e) { return Task.CompletedTask; }
        protected virtual async Task SetDefaultClaims(TicketReceivedContext e, Auth0User user)
        {
            // Set the remote avatar on a local claim, in case the local overrides it. 
            if (e.Principal.HasClaim(HoodClaimTypes.Picture))
            {
                e.Principal.AddOrUpdateClaimValue(HoodClaimTypes.RemotePicture, e.Principal.GetClaimValue(HoodClaimTypes.Picture));
            }

            // Set the user claims locally
            e.Principal.SetUserClaims(user.UserProfile);
            await e.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, e.Principal, e.Properties);
        }

    }
}
