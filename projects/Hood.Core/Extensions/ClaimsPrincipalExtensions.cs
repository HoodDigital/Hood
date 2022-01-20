using Hood.Core;
using Hood.Identity;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using System;
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
                identity.AddClaim(new Claim(HoodClaimTypes.LocalUserId, user.Id));
            }

            if (!identity.Name.IsValidEmail())
            {
                // social login has put the name as the auth0 id.                
                var username = identity.FindFirst("name");
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
            if (user.FirstName.IsSet())
            {
                var givenName = identity.FindFirst(ClaimTypes.GivenName);
                if (givenName != null)
                {
                    identity.RemoveClaim(givenName);
                }
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            }

            if (user.LastName.IsSet())
            {
                var surname = identity.FindFirst(ClaimTypes.Surname);
                if (surname != null)
                {
                    identity.RemoveClaim(surname);
                }
                identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
            }

            if (user.DisplayName.IsSet())
            {
                var nickname = identity.FindFirst("nickname");
                if (nickname != null)
                {
                    identity.RemoveClaim(nickname);
                }
                identity.AddClaim(new Claim("nickname", user.DisplayName));
            }

            identity.AddClaim(new Claim(HoodClaimTypes.InternalName, user.ToAdminName()));
            identity.AddClaim(new Claim(HoodClaimTypes.DisplayName, user.ToDisplayName()));

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
        public static string ToDisplayName(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst(HoodClaimTypes.DisplayName);

            return claim?.Value;
        }
        public static string ToInternalName(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst(HoodClaimTypes.InternalName);

            return claim?.Value;
        }
        public static string GetLocalUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            Claim claim = principal.FindFirst(HoodClaimTypes.LocalUserId);
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
            Claim claim = principal.FindFirst(ClaimTypes.NameIdentifier);

            return claim?.Value;
        }
        public static bool IsNotSetup(this ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var username = identity.FindFirst(HoodClaimTypes.AccountNotConnected);
            if (username != null)
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
