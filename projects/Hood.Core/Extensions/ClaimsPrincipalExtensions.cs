using Hood.Core;
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

        public static void SetUserClaims(this ClaimsPrincipal principal, ApplicationUser user)
        {
            var identity = (ClaimsIdentity)principal.Identity;

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

            identity.AddClaim(new Claim("adminname", user.ToAdminName()));
            identity.AddClaim(new Claim("displayname", user.ToDisplayName()));
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
            Claim claim = principal.FindFirst("displayname");

            return claim?.Value;
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
