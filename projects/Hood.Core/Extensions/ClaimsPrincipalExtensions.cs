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
        public static bool HasClaim(this ClaimsPrincipal principal, string claimName)
        {
            return principal.GetClaim(claimName) != null;
        }
        public static string GetClaim(this ClaimsPrincipal principal, string claimName)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var claim = identity.FindFirst(claimName);
            return claim?.Value;
        }
        public static ClaimsPrincipal RemoveClaim(this ClaimsPrincipal principal, string claimName)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var claim = identity.FindFirst(claimName);
            if (claim != null)
            {
                identity.RemoveClaim(claim);
            }
            return principal;
        }
        public static ClaimsPrincipal AddOrUpdateClaim(this ClaimsPrincipal principal, string claimName, string value)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var claim = identity.FindFirst(claimName);
            if (claim != null)
            {
                identity.RemoveClaim(claim);
            }
            identity.AddClaim(new Claim(claimName, value));
            return principal;
        }

        public static void SetUserClaims(this ClaimsPrincipal principal, IUserProfile user)
        {

            // ensure the local user id is stored in case we are using an external auth account
            var userId = principal.GetUserId();
            if (user.Id != userId)
            {
                principal.AddOrUpdateClaim(Identity.HoodClaimTypes.LocalUserId, user.Id);
            }

            // Make sure the User.Identity.Name is set to the user's email.
            principal.AddOrUpdateClaim(HoodClaimTypes.UserName, user.Email);

            // Set the picture -if one is set in the user, then add the url to picture claim.
            if (user.AvatarJson.IsSet())
            {
                principal.AddOrUpdateClaim(HoodClaimTypes.Picture, user.Avatar.LargeUrl);
            }

            // Set name/displayname - if set in the local user overwrite the claims. 
            if (user.FirstName.IsSet())
            {
                principal.AddOrUpdateClaim(ClaimTypes.GivenName, user.FirstName);
            }

            if (user.LastName.IsSet())
            {
                principal.AddOrUpdateClaim(ClaimTypes.Surname, user.LastName);
            }

            principal.RemoveClaim(HoodClaimTypes.Nickname);
            if (user.DisplayName.IsSet())
            {
                principal.AddOrUpdateClaim(HoodClaimTypes.Nickname, user.DisplayName);
            }

            principal.AddOrUpdateClaim(Identity.HoodClaimTypes.Anonymous, user.Anonymous.ToString());
        }
        public static string GetAvatar(this ClaimsPrincipal principal)
        {
            return principal.GetClaim(HoodClaimTypes.Picture);
        }
        public static bool IsEmailConfirmed(this ClaimsPrincipal principal)
        {
            return principal.GetClaim(HoodClaimTypes.EmailConfirmed)?.ToLower() == "true";
        }
        public static bool IsAnonymous(this ClaimsPrincipal principal)
        {
            return principal.GetClaim(HoodClaimTypes.Anonymous)?.ToLower() == "true";
        }
        public static string ToDisplayName(this ClaimsPrincipal principal, bool allowAnonymous = true)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            bool anonymous = principal.IsAnonymous();
            string displayName = principal.GetClaim(HoodClaimTypes.Nickname);
            string firstName = principal.GetClaim(ClaimTypes.GivenName);
            string lastName = principal.GetClaim(ClaimTypes.Surname);

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
            Claim claim = principal.FindFirst(ClaimTypes.Email);

            return claim?.Value;
        }
        public static string ToInternalName(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            bool anonymous = principal.IsAnonymous();
            string displayName = principal.GetClaim(HoodClaimTypes.Nickname);
            string firstName = principal.GetClaim(ClaimTypes.GivenName);
            string lastName = principal.GetClaim(ClaimTypes.Surname);
            string email = principal.GetClaim(ClaimTypes.Email);

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
            Claim claim = principal.FindFirst(ClaimTypes.NameIdentifier);

            return claim?.Value;
        }
        public static bool RequiresConnection(this ClaimsPrincipal principal)
        {
            return principal.GetClaim(HoodClaimTypes.AccountNotConnected) != null;
        }
        public static bool IsActive(this ClaimsPrincipal principal)
        {
            return principal.GetClaim(HoodClaimTypes.Active) != null;
        }
        public static bool IsImpersonating(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            return principal.HasClaim(HoodClaimTypes.IsImpersonating, "true");
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
