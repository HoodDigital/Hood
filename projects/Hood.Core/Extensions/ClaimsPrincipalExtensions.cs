﻿using Hood.Identity;
using Hood.Models;
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
            return principal.GetClaimValue(claimName) != null;
        }
        public static string GetClaimValue(this ClaimsPrincipal principal, string claimName)
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
        public static ClaimsPrincipal AddClaim(this ClaimsPrincipal principal, string claimName, string value)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var claims = identity.FindAll(claimName);
            var claimValues = claims.Select(r => r.Value).ToList();
            if (!claimValues.Contains(value))
            {
                identity.AddClaim(new Claim(claimName, value));
            }
            return principal;
        }
        public static ClaimsPrincipal AddOrUpdateClaimValue(this ClaimsPrincipal principal, string claimName, string value)
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
                principal.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.LocalUserId, user.Id);
            }

            // Make sure the User.Identity.Name is set to the user's email.
            principal.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.UserName, user.Email);

            // Set the picture -if one is set in the user, then add the url to picture claim.
            if (user.AvatarJson.IsSet())
            {
                principal.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.Picture, user.Avatar.LargeUrl);
            }

            // Set name/displayname - if set in the local user overwrite the claims. 
            if (user.FirstName.IsSet())
            {
                principal.AddOrUpdateClaimValue(ClaimTypes.GivenName, user.FirstName);
            }

            if (user.LastName.IsSet())
            {
                principal.AddOrUpdateClaimValue(ClaimTypes.Surname, user.LastName);
            }

            principal.RemoveClaim(Hood.Constants.Identity.ClaimTypes.Nickname);
            if (user.DisplayName.IsSet())
            {
                principal.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.Nickname, user.DisplayName);
            }

            principal.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.Anonymous, user.Anonymous.ToString());
        }
        public static string GetAvatar(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(Hood.Constants.Identity.ClaimTypes.Picture);
        }
        public static bool IsEmailConfirmed(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(Hood.Constants.Identity.ClaimTypes.EmailConfirmed)?.ToLower() == "true";
        }
        public static bool IsAnonymous(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(Hood.Constants.Identity.ClaimTypes.Anonymous)?.ToLower() == "true";
        }
        public static string ToDisplayName(this ClaimsPrincipal principal, bool allowAnonymous = true)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            bool anonymous = principal.IsAnonymous();
            string displayName = principal.GetClaimValue(Hood.Constants.Identity.ClaimTypes.Nickname);
            string firstName = principal.GetClaimValue(ClaimTypes.GivenName);
            string lastName = principal.GetClaimValue(ClaimTypes.Surname);

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
            string displayName = principal.GetClaimValue(Hood.Constants.Identity.ClaimTypes.Nickname);
            string firstName = principal.GetClaimValue(ClaimTypes.GivenName);
            string lastName = principal.GetClaimValue(ClaimTypes.Surname);
            string email = principal.GetClaimValue(ClaimTypes.Email);

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
            Claim claim = principal.FindFirst(Hood.Constants.Identity.ClaimTypes.LocalUserId);
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
            return principal.GetClaimValue(Hood.Constants.Identity.ClaimTypes.AccountNotConnected) != null;
        }
        public static bool IsActive(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(Hood.Constants.Identity.ClaimTypes.Active) != null;
        }
        public static bool IsImpersonating(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            return principal.HasClaim(Hood.Constants.Identity.ClaimTypes.IsImpersonating, "true");
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
