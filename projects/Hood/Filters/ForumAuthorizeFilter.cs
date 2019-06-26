using Hood.Caching;
using Hood.Core;
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

            private readonly ForumAccess _access;

            public ForumAuthorizeFilterImpl(
                HoodDbContext db,
               ForumAccess access)
            {
                _db = db;
                _access = access;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                ForumSettings _forumSettings = Engine.Settings.Forum;

                // if user is admin or moderator - let them through, regardless.
                if (context.HttpContext.User.IsForumModerator())
                    return;

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
                if ((
                    _forumSettings.PostingRequiresSubscription ||
                    _forumSettings.ViewingRequiresSubscription ||
                    _forumSettings.PostingRequiresSpecificSubscription ||
                    _forumSettings.ViewingRequiresSpecificSubscription
                ) && !Engine.Settings.Billing.EnableSubscriptions)
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
                ) && !Engine.Settings.Billing.EnableSubscriptions)
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

                    // if is forum owner
                    if (forum.AuthorId == _account.User.Id)
                        moderator = true;

                    // if this is a post (postId is a route variable)
                    // Specific forum based subscription/role required stuff
                    if (context.RouteData.Values.ContainsKey("postId"))
                    {
                        if (long.TryParse(context.RouteData.Values["postId"].ToString(), out long postId))
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
                        if (int.TryParse(context.RouteData.Values["topicId"].ToString(), out int topicId))
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
