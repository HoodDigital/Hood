using Hood.Caching;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Filters
{
    /// <summary>
    /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, then it checks what is required for forum access and fires the user to the right pages to upgrade if required.
    /// </summary>
    /// <param name="AccessRequired">Determines what type of access is required.</param>
    public class ForumAuthorizeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, then it checks what is required for forum access and fires the user to the right pages to upgrade if required.
        /// </summary>
        /// <param name="AccessRequired">Determines what type of access is required.</param>
        public ForumAuthorizeAttribute(ForumAccess AccessRequired = ForumAccess.View) : base(typeof(ForumAuthorizeFilterImpl))
        {
            Arguments = new object[] { AccessRequired };
        }

        private class ForumAuthorizeFilterImpl : IActionFilter
        {
            private readonly HoodDbContext _db;
            private readonly ILogger _logger;
            private readonly IBillingService _billing;
            private readonly ISettingsRepository _settings;

            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IAccountRepository _auth;
            private readonly RoleManager<IdentityRole> _roleManager;

            private readonly ForumAccess _access;

            public ForumAuthorizeFilterImpl(
                HoodDbContext db,
                IAccountRepository auth,
                ILoggerFactory loggerFactory,
                IBillingService billing,
                IHttpContextAccessor contextAccessor,
                IHoodCache cache,
                ISettingsRepository settings,
                RoleManager<IdentityRole> roleManager,
                UserManager<ApplicationUser> userManager,
               ForumAccess access)
            {
                _db = db;
                _auth = new AccountRepository(db, settings, billing, contextAccessor, cache, userManager, roleManager);
                _logger = loggerFactory.CreateLogger<SubscriptionRequiredAttribute>();
                _billing = billing;
                _settings = settings;
                _userManager = userManager;
                _roleManager = roleManager;
                _access = access;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                ForumSettings _forumSettings = _settings.GetForumSettings();

                // Login required stuff
                IActionResult loginResult = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent() });

                if (!context.HttpContext.User.Identity.IsAuthenticated && _access == ForumAccess.View && _forumSettings.ViewingRequiresLogin)
                {
                    context.Result = loginResult;
                    return;
                }
                if (!context.HttpContext.User.Identity.IsAuthenticated && _access == ForumAccess.Post && _forumSettings.PostingRequiresLogin)
                {
                    context.Result = loginResult;
                    return;
                }

                AccountInfo _account = context.HttpContext.GetAccountInfo();

                // Subscription required stuff
                var subscriptionsEnabled = _settings.SubscriptionsEnabled();
                if ((
                    _forumSettings.PostingRequiresSubscription ||
                    _forumSettings.ViewingRequiresSubscription ||
                    _forumSettings.PostingRequiresSpecificSubscription ||
                    _forumSettings.ViewingRequiresSpecificSubscription
                ) && !subscriptionsEnabled.Succeeded)
                {
                    throw new Exception("You must enable subscriptions before you can set required subscriptions for forums.");
                }

                IActionResult unauthorizedResult = new RedirectToActionResult("Unauthorized", "Forum", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent() });

                // Check if forums in general can be viewed without subscription, of not check for a tier sub
                if (!_account.HasTieredSubscription && _access == ForumAccess.View && _forumSettings.ViewingRequiresSubscription)
                {
                    if (CheckForOverrideRoles(_forumSettings.ViewingRoles, context))
                        return;

                    context.Result = unauthorizedResult;
                    return;
                }

                // Check if forums in general can be posted in without subscription, of not check for a tier sub
                if (!_account.HasTieredSubscription && _access == ForumAccess.Post && _forumSettings.PostingRequiresSubscription)
                {
                    if (CheckForOverrideRoles(_forumSettings.PostingRoles, context))
                        return;

                    context.Result = unauthorizedResult;
                    return;
                }

                // Specific subscription/role required stuff

                // Check if forums in general can be viewed without a SPECIFIC subscription, if one is required, make sure the user is in that subscription
                if (_access == ForumAccess.View && _forumSettings.ViewingRequiresSpecificSubscription)
                {
                    if (!_forumSettings.ViewingSubscriptionList.Any(s => _account.IsSubscribed(s)))
                    {
                        // if the user is in an override role (code access for example, or role granted by admin)
                        if (CheckForOverrideRoles(_forumSettings.ViewingRoles, context))
                            return;
                        context.Result = unauthorizedResult;
                        return;
                    }
                }

                // Check if forums in general can be posted in without a SPECIFIC subscription, if one is required, make sure the user is in that subscription
                if (_access == ForumAccess.Post && _forumSettings.PostingRequiresSpecificSubscription)
                {
                    if (!_forumSettings.PostingSubscriptionList.Any(s => _account.IsSubscribed(s)))
                    {
                        // if the user is in an override role (code access for example, or role granted by admin)
                        if (CheckForOverrideRoles(_forumSettings.PostingRoles, context))
                            return;
                        context.Result = unauthorizedResult;
                        return;
                    }
                }

                // Specific forum based subscription/role required stuff
                if (!context.RouteData.Values.ContainsKey("slug"))
                    return;

                string slug = context.RouteData.Values["slug"].ToString();
                if (!slug.IsSet())
                    return;

                Forum forum = _db.Forums.SingleOrDefault(f => f.Slug == slug);
                if (forum == null)
                    return;

                if (!context.HttpContext.User.Identity.IsAuthenticated && _access == ForumAccess.View && forum.ViewingRequiresLogin)
                {
                    context.Result = loginResult;
                    return;
                }

                if (!context.HttpContext.User.Identity.IsAuthenticated && _access == ForumAccess.Post && forum.PostingRequiresLogin)
                {
                    context.Result = loginResult;
                    return;
                }


                if ((
                    forum.PostingRequiresSubscription ||
                    forum.ViewingRequiresSubscription ||
                    forum.PostingRequiresSpecificSubscription ||
                    forum.ViewingRequiresSpecificSubscription
                ) && !subscriptionsEnabled.Succeeded)
                {
                    throw new Exception("You must enable subscriptions before you can set required subscriptions for forums.");
                }

                // Check if the current forum can be viewed without subscription, of not check for a tier sub
                if (!_account.HasTieredSubscription && _access == ForumAccess.View && forum.ViewingRequiresSubscription)
                {
                    if (CheckForOverrideRoles(_forumSettings.ViewingRoles, context))
                        return;
                    if (CheckForOverrideRoles(forum.ViewingRoles, context))
                        return;

                    context.Result = unauthorizedResult;
                    return;
                }

                // Check if the current forum can be posted in without subscription, of not check for a tier sub
                if (!_account.HasTieredSubscription && _access == ForumAccess.Post && forum.PostingRequiresSubscription)
                {
                    if (CheckForOverrideRoles(_forumSettings.PostingRoles, context))
                        return;
                    if (CheckForOverrideRoles(forum.PostingRoles, context))
                        return;

                    context.Result = unauthorizedResult;
                    return;
                }

                // Check if the current forum can be viewed without a SPECIFIC subscription, if one is required, make sure the user is in that subscription
                if (_access == ForumAccess.View && forum.ViewingRequiresSpecificSubscription)
                {
                    if (!forum.ViewingSubscriptionList.Any(s => _account.IsSubscribed(s)))
                    {
                        // if the user is in an override role (code access for example, or role granted by admin)
                        if (CheckForOverrideRoles(_forumSettings.ViewingRoles, context))
                            return;
                        if (CheckForOverrideRoles(forum.ViewingRoles, context))
                            return;
                        context.Result = unauthorizedResult;
                        return;
                    }
                }

                // Check if the current forum can be posted in without a SPECIFIC subscription, if one is required, make sure the user is in that subscription
                if (_access == ForumAccess.Post && forum.PostingRequiresSpecificSubscription)
                {
                    if (!forum.PostingSubscriptionList.Any(s => _account.IsSubscribed(s)))
                    {
                        // if the user is in an override role (code access for example, or role granted by admin)
                        if (CheckForOverrideRoles(_forumSettings.PostingRoles, context))
                            return;
                        if (CheckForOverrideRoles(forum.PostingRoles, context))
                            return;
                        context.Result = unauthorizedResult;
                        return;
                    }
                }

                if (_access == ForumAccess.Moderate)
                {
                    bool moderator = false;
                    // moderator checks, if admin or mod, let through.
                    if (_userManager.IsInRoleAsync(_account.User, "Administrator").Result ||
                        _userManager.IsInRoleAsync(_account.User, "Moderator").Result)
                        moderator = true;

                    // if is forum owner
                    if (forum.AuthorId == _account.User.Id)
                        moderator = true;

                    // if this is a post (postId is a route variable)
                    // Specific forum based subscription/role required stuff
                    if (context.RouteData.Values.ContainsKey("postId"))
                    {
                        long postId = 0;
                        if (long.TryParse(context.RouteData.Values["postId"].ToString(), out postId))
                        {
                            Post post = _db.Posts.SingleOrDefault(f => f.Id == postId);
                            if (post != null)
                            {
                                // if user is author, allow through.
                                if (post.AuthorId == _account.User.Id)
                                    moderator = true;
                            }
                        }
                    }

                    // if this is a topic (topicId is a route variable)
                    // check for user access (Author)
                    if (context.RouteData.Values.ContainsKey("topicId"))
                    {
                        int topicId = 0;
                        if (int.TryParse(context.RouteData.Values["topicId"].ToString(), out topicId))
                        {
                            Topic topic = _db.Topics.SingleOrDefault(f => f.Id == topicId);
                            if (topic != null)
                            {
                                // if user is author, allow through.
                                if (topic.AuthorId == _account.User.Id)
                                    moderator = true;
                            }
                        }
                    }

                    if (!moderator)
                    {
                        context.Result = unauthorizedResult;
                        return;
                    }
                }

            }

            private bool CheckForOverrideRoles(string roles, ActionExecutingContext context)
            {
                if (roles.IsSet())
                {
                    List<string> roleList = JsonConvert.DeserializeObject<List<string>>(roles);
                    if (roleList != null)
                        if (roleList.Count > 0)
                        {
                            if (roleList.Any(s => context.HttpContext.User.IsInRole(s)))
                            {
                                return true;
                            }
                        }
                }
                return false;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
