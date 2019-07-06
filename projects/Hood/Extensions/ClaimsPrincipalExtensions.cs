using Hood.Core;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;

namespace Hood.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static UserSubscriptionsView AccountInfo(this ClaimsPrincipal principal)
        {
            if (!principal.Identity.IsAuthenticated)
                return null;

            principal.GetUserId();
            var contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();

            var view = contextAccessor.HttpContext.Items[nameof(UserSubscriptionsView)] as UserSubscriptionsView;
            if (view == null)
            {
                var context = Engine.Services.Resolve<HoodDbContext>();
                view = context.UserSubscriptionView.SingleOrDefault(us => us.Id == principal.GetUserId());
                if (view != null)
                    contextAccessor.HttpContext.Items[nameof(UserSubscriptionsView)] = view;
            }

            return view;
        }

        public static bool IsSubscribed(this ClaimsPrincipal principal, string id)
        {
            var view = AccountInfo(principal);

            if (view == null)
                return false;

            if (view.Subscriptions == null)
                return false;

            return view.Subscriptions.Any(a => a.StripeId == id && (a.Status == "active" || a.Status == "trialing"));
        }

        public static bool HasActiveSubscription(this ClaimsPrincipal principal)
        {
            var view = AccountInfo(principal);

            if (view == null)
                return false;

            if (view.Subscriptions == null)
                return false;

            return view.Subscriptions.Any(a => !a.Addon && (a.Status == "active" || a.Status == "trialing"));
        }

        public static bool IsSubscribedToCategory(this ClaimsPrincipal principal, string category)
        {
            var view = AccountInfo(principal);

            if (view == null)
                return false;

            if (view.Subscriptions == null)
                return false;

            return view.Subscriptions.Any(a => a.Category.Contains(category) && (a.Status == "active" || a.Status == "trialing"));
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
