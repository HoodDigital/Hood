using Hood.Core;
using Hood.Identity;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Hood.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static void SetUserClaims(this ClaimsPrincipal principal, IUserProfile user)
        {
            var identity = (ClaimsIdentity)principal.Identity;

            // ensure the local user id is stored in case we are using an external auth account
            var userId = principal.GetUserId();
            if (user.Id != userId)
            {
                identity.AddClaim(new Claim(Identity.HoodClaimTypes.LocalUserId, user.Id));
            }

            var username = identity.FindFirst("name");
            if (!identity.Name.IsValidEmail())
            {
                // social login has put the name as the auth0 id.         
                if (username != null)
                {
                    identity.RemoveClaim(username);
                }
                identity.AddClaim(new Claim("name", user.Email));
            }

            // Set the picture -if one is set in the user, then add the url to picture claim.
            if (user.AvatarJson.IsSet())
            {
                identity.RemoveClaim(identity.FindFirst("picture"));
                identity.AddClaim(new Claim("picture", user.Avatar.LargeUrl));
            }

            // Set name/displayname - if set in the local user overwrite the claims. 
            var givenName = identity.FindFirst(System.Security.Claims.ClaimTypes.GivenName);
            if (user.FirstName.IsSet())
            {
                if (givenName != null)
                {
                    identity.RemoveClaim(givenName);
                }
                identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.GivenName, user.FirstName));
            }

            var surname = identity.FindFirst(System.Security.Claims.ClaimTypes.Surname);
            if (user.LastName.IsSet())
            {
                if (surname != null)
                {
                    identity.RemoveClaim(surname);
                }
                identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.Surname, user.LastName));
            }

            var nickname = identity.FindFirst("nickname");
            if (nickname != null)
            {
                identity.RemoveClaim(nickname);
            }
            if (user.DisplayName.IsSet())
            {
                identity.AddClaim(new Claim("nickname", user.DisplayName));
            }

            var anonymous = identity.FindFirst(Identity.HoodClaimTypes.Anonymous);
            if (anonymous != null)
            {
                identity.RemoveClaim(anonymous);
            }
            identity.AddClaim(new Claim(Identity.HoodClaimTypes.Anonymous, user.Anonymous.ToString()));

        }
        public static string GetAvatar(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst("picture");

            return claim?.Value;
        }
        public static bool IsEmailConfirmed(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst("picture");

            return (claim?.Value == "true");
        }
        public static bool IsAnonymous(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst(Identity.HoodClaimTypes.Anonymous);

            return (claim?.Value == "True");
        }
        public static string ToDisplayName(this ClaimsPrincipal principal, bool allowAnonymous = true)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            bool anonymous = principal.IsAnonymous();
            string displayName = principal.FindFirst("nickname")?.Value;
            string firstName = principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
            string lastName = principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;

            if (anonymous && allowAnonymous)
                return "Anonymous";
            if (displayName.IsSet())
                return displayName;
            return principal.ToInternalName();
        }
        public static string GetEmail(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst(System.Security.Claims.ClaimTypes.Email);

            return claim?.Value;
        }
        public static string ToInternalName(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            bool anonymous = principal.IsAnonymous();
            string displayName = principal.FindFirst("nickname")?.Value;
            string firstName = principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
            string lastName = principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;
            string email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (firstName.IsSet() && lastName.IsSet())
                return firstName + " " + lastName;
            else if (firstName.IsSet() && !lastName.IsSet())
                return firstName;
            else if (!firstName.IsSet() && lastName.IsSet())
                return lastName;
            else return email;
        }
        public static string GetLocalUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst(Identity.HoodClaimTypes.LocalUserId);
            if (claim != null)
            {
                return claim.Value;
            }
            return principal.GetUserId();
        }
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            return claim?.Value;
        }
        public static bool RequiresConnection(this ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var accountConnected = identity.FindFirst(Identity.HoodClaimTypes.AccountNotConnected);
            if (accountConnected != null)
            {
                return true;
            }
            return false;
        }
        public static bool IsActive(this ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var accountConnected = identity.FindFirst(Identity.HoodClaimTypes.Active);
            if (accountConnected != null)
            {
                return true;
            }
            return false;
        }
        public static bool IsImpersonating(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            bool isImpersonating = principal.HasClaim("IsImpersonating", "true");
            return isImpersonating;
        }        
        public static List<string> GetRoles(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            
            var identity = (ClaimsIdentity)principal.Identity;
            var roles = identity.FindAll(System.Security.Claims.ClaimTypes.Role);
            return roles.Select(r => r.Value).ToList();
        }
        public static bool IsForumModerator(this ClaimsPrincipal principal)
        {
            return principal.IsEditorOrBetter() || principal.IsInRole("Forum");
        }
        public static bool IsEditorOrBetter(this ClaimsPrincipal principal)
        {
            return principal.IsAdminOrBetter() || principal.IsInRole("Editor");
        }
        public static bool IsAdminOrBetter(this ClaimsPrincipal principal)
        {
            return principal.IsSuperUser() || principal.IsInRole("Admin");
        }
        public static bool IsSuperUser(this ClaimsPrincipal principal)
        {
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            return principal.IsInRole("SuperUser");
        }
    }
}
