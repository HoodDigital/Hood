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
                return null;

            principal.GetUserId();
            var contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();

            var profile = contextAccessor.HttpContext.Items[nameof(UserProfile)] as UserProfile;
            if (profile == null)
            {
                var context = Engine.Services.Resolve<HoodDbContext>();
                profile = context.UserProfiles.SingleOrDefault(us => us.Id == principal.GetUserId());
                if (profile != null)
                    contextAccessor.HttpContext.Items[nameof(UserProfile)] = profile;
            }

            return profile;
        }

        public static bool IsSubscribed(this ClaimsPrincipal principal, string stripeId)
        {
            var profile = GetUserProfile(principal);

            if (profile == null)
                return false;

            if (profile.Subscriptions == null)
                return false;

            return profile.ActiveSubscriptions.Any(a => a.StripeId == stripeId);
        }
        public static bool IsSubscribed(this ClaimsPrincipal principal, int planId)
        {
            var profile = GetUserProfile(principal);

            if (profile == null)
                return false;

            if (profile.Subscriptions == null)
                return false;

            return profile.ActiveSubscriptions.Any(a => a.PlanId == planId);
        }

        public static bool HasActiveSubscription(this ClaimsPrincipal principal)
        {
            var profile = GetUserProfile(principal);

            if (profile == null)
                return false;

            if (profile.Subscriptions == null)
                return false;

            return profile.ActiveSubscriptions.Any();
        }

        public static bool IsSubscribedToProduct(this ClaimsPrincipal principal, int productId)
        {
            var profile = GetUserProfile(principal);

            if (profile == null)
                return false;

            if (profile.Subscriptions == null)
                return false;

            return profile.ActiveSubscriptions.Any(a => a.SubscriptionProductId == productId);
        }
        public static UserSubscriptionInfo GetActiveSubscription(this ClaimsPrincipal principal, int productId)
        {
            var profile = GetUserProfile(principal);

            if (profile == null)
                return null;

            if (profile.Subscriptions == null)
                return null;

            return profile.ActiveSubscriptions.FirstOrDefault(a => a.SubscriptionProductId == productId);
        }

        // https://stackoverflow.com/a/35577673/809357
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);

            return claim?.Value;
        }

        public static bool IsImpersonating(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var isImpersonating = principal.HasClaim("IsImpersonating", "true");

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
            if (!principal.Identity.IsAuthenticated) return false;
            return principal.IsInRole("SuperUser");
        }
    }
}
