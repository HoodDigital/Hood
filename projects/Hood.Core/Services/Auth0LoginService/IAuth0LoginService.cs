using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Net.Http;

namespace Hood.Services
{
    public interface IAuth0LoginService
    {

#nullable enable

        string Domain { get; set; }
        string ClientId { get; set; }
        string? ClientSecret { get; set; }
        string Scope { get; set; }
        string? CallbackPath { get; set; }
        string? Organization { get; set; }
        IDictionary<string, string>? LoginParameters { get; set; }
        OpenIdConnectEvents? OpenIdConnectEvents { get; set; }
        string? ResponseType { get; set; }
        HttpClient? Backchannel { get; set; }
        TimeSpan? MaxAge { get; set; }

#nullable disable

        OpenIdConnectEvents AsOpenIdConnectEvents();
        Task OnAccessDenied(AccessDeniedContext e);
        Task OnAuthenticationFailed(AuthenticationFailedContext e);
        Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext e);
        Task OnMessageReceived(MessageReceivedContext e);
        Task OnRedirectToIdentityProvider(RedirectContext e);
        Task OnRedirectToIdentityProviderForSignOut(RedirectContext e);
        Task OnRemoteFailure(RemoteFailureContext e);
        Task OnRemoteSignOut(RemoteSignOutContext e);
        Task OnSignedOutCallbackRedirect(RemoteSignOutContext e);
        Task OnTicketReceived(TicketReceivedContext e);
        Task OnTokenResponseReceived(TokenResponseReceivedContext e);
        Task OnTokenValidated(TokenValidatedContext e);
        Task OnUserInformationReceived(UserInformationReceivedContext e);
    }
}
