using Hood.Core;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace Hood.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static UserProfile GetUserProfile(this ClaimsPrincipal principal)
        {
            if (!principal.Identity.IsAuthenticated)
            {
                return null;
            }
            
            IHttpContextAccessor contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
            UserProfile profile = contextAccessor.HttpContext.Items[nameof(UserProfile)] as UserProfile;
            if (profile == null)
            {
                HoodDbContext context = Engine.Services.Resolve<HoodDbContext>();
                try
                {
                    profile = context.UserProfiles.SingleOrDefault(us => us.Email == principal.Identity.Name);
                }
                catch (Exception)
                {

                }
                if (profile != null)
                {
                    contextAccessor.HttpContext.Items[nameof(UserProfile)] = profile;
                }
            }

            return profile;
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
