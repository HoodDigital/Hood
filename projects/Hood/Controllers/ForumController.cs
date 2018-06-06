using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.IO;
using Hood.Models;
using Hood.Filters;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hood.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Controllers
{
    public class ForumController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HoodDbContext _db;
        private readonly ForumCategoryCache _categories;
        private readonly IEmailSender _email;
        private readonly ISettingsRepository _settings;
        private readonly IHostingEnvironment _environment;
        private readonly IMediaManager<MediaObject> _media;
        private readonly IAccountRepository _auth;

        public ForumController(
            IAccountRepository auth,
            IEmailSender email,
            HoodDbContext db,
            ForumCategoryCache categories,
            UserManager<ApplicationUser> userManager,
            ISettingsRepository settings,
            IBillingService billing,
            IMediaManager<MediaObject> media,
            IHostingEnvironment env)
        {
            _auth = auth;
            _media = media;
            _userManager = userManager;
            _db = db;
            _settings = settings;
            _environment = env;
            _categories = categories;
            _email = email;
        }

        #region "Forum Views"

        [Route("forums/")]
        [ForumAuthorize(ForumAccess.View)]
        public async Task<IActionResult> Index(ForumModel model, ForumMessage? message)
        {
            IQueryable<Forum> forums = _db.Forums
                .Include(f => f.Author)
                .Include(f => f.Categories).ThenInclude(c => c.Category);

            if (model.Category.IsSet())
            {
                forums = forums.Where(c => c.Categories.Any(cat => cat.Category.Slug == model.Category));
            }

            if (!string.IsNullOrEmpty(model.Search))
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                forums = forums.Where(n => searchTerms.Any(s => n.Title != null && n.Title.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Description != null && n.Description.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }

            if (!string.IsNullOrEmpty(model.Order))
            {
                switch (model.Order)
                {
                    case "name":
                    case "title":
                        forums = forums.OrderBy(n => n.Title);
                        break;
                    case "date":
                        forums = forums.OrderBy(n => n.CreatedOn);
                        break;
                    case "latest":
                        forums = forums.OrderBy(n => n.LastPosted);
                        break;

                    case "name+desc":
                    case "title+desc":
                        forums = forums.OrderByDescending(n => n.Title);
                        break;
                    case "date+desc":
                        forums = forums.OrderByDescending(n => n.CreatedOn);
                        break;
                    case "latest+desc":
                        forums = forums.OrderByDescending(n => n.LastPosted);
                        break;

                    default:
                        forums = forums.OrderByDescending(n => n.CreatedOn).ThenBy(n => n.Id);
                        break;
                }
            }

            await model.ReloadAsync(forums);
            model.AddForumMessage(message);
            return View(model);
        }

        [Route("forum/{slug}")]
        [ForumAuthorize(ForumAccess.View)]
        public async Task<IActionResult> Topics(TopicModel model, ForumMessage? message)
        {
            if (model.Forum == null)
                model.Forum = await _db.Forums
                        .Include(f => f.Author)
                        .Include(f => f.Categories).ThenInclude(c => c.Category)
                        .SingleOrDefaultAsync(f => f.Slug == model.Slug);

            if (model == null || model.Forum == null)
                return NotFound();

            if (!model.Forum.Published)
                return View("Offline", model);

            IQueryable<Topic> topics = _db.Topics
                .Include(t => t.Author)
                .Include(t => t.Forum)
                .Where(t => t.ForumId == model.Forum.Id);

            if (!string.IsNullOrEmpty(model.Search))
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                topics = topics.Where(t => searchTerms.Any(s => t.Title != null && t.Title.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => t.Description != null && t.Description.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }

            await model.ReloadAsync(topics);

            if (!model.SaveMessage.IsSet())
                model.AddForumMessage(message);

            return View("Topics", model);
        }

        [HttpPost]
        [ForumAuthorize(ForumAccess.Post)]
        [Route("forum/{slug}")]
        public async Task<IActionResult> AddTopic(TopicModel model)
        {
            try
            {
                model.Forum = await _db.Forums
                        .Include(f => f.Author)
                        .Include(f => f.Categories).ThenInclude(c => c.Category)
                        .SingleOrDefaultAsync(f => f.Slug == model.Slug);

                if (model == null || model.Forum == null)
                    return NotFound();

                if (!model.Forum.Published)
                    return View("Offline", model);

                // check the topic - if its not up to snuff dont use it, just fire an error and pass the model on.
                if (model.Topic == null)
                {
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "No topic was submitted.";
                }

                if (!model.Topic.Title.IsSet())
                {
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "You have to enter a title.";
                }

                if (!model.Topic.Description.IsSet())
                {
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "You have to enter a description.";
                }
                var user = await _userManager.GetUserAsync(User);
                model.Topic.AuthorId = user.Id;
                model.Topic.AuthorDisplayName = user.DisplayName;
                model.Topic.AuthorName = user.UserName;
                model.Topic.ForumId = model.Forum.Id;
                model.Topic.CreatedOn = DateTime.Now;

                if (model.Forum.RequireTopicModeration)
                    model.Topic.Approved = false;
                else
                    model.Topic.Approved = true;

                _db.Topics.Add(model.Topic);
                await _db.SaveChangesAsync();

                model.Forum.NumTopics = await _db.Topics.CountAsync(f => f.ForumId == model.Topic.ForumId);
                await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                model.MessageType = AlertType.Danger;
                model.SaveMessage = "An error occured while adding your message: " + ex.Message;
            }

            return await Topics(model, ForumMessage.Created);
        }

        [Route("forum/{slug}/{topicId}/{title}/")]
        [ForumAuthorize(ForumAccess.View)]
        public async Task<IActionResult> ShowTopic(PostModel model, ForumMessage? message)
        {
            if (model.Topic == null)
                model.Topic = await _db.Topics
                        .Include(f => f.Author)
                        .Include(f => f.Forum)
                        .SingleOrDefaultAsync(f => f.Id == model.TopicId);


            if (model == null || model.Topic == null || model.Topic.Forum == null)
                return NotFound();

            if (!model.Topic.Forum.Published)
                return View("Offline", model);

            IQueryable<Post> posts = _db.Posts
                .Include(p => p.Author)
                .Include(p => p.Topic).ThenInclude(t => t.Forum)
                .Where(p => p.TopicId == model.TopicId);

            // Prep for input.
            model.Post = new Post();

            if (model.ReplyId.HasValue)
            {
                model.Post.Reply = await posts.SingleOrDefaultAsync(p => p.Id == model.ReplyId.Value);
                if (model.Post.Reply != null)
                {
                    model.Post.ReplyId = model.ReplyId;
                    model.Post.Body = string.Format("<blockquote>{0}</blockquote><p></p>", model.Post.Reply.Body);
                }
            }

            if (model.HighlightId.HasValue)
            {
                // page number needs to be set from the highlight. F*** the set pagenumber.
                int index = posts.Select(p => p.Id).ToList().FindIndex(p => p == model.HighlightId.Value);
                model.PageIndex = 1 + (index / model.PageSize);
            }
            else
            {
                // process list as normal
                if (!string.IsNullOrEmpty(model.Search))
                {
                    string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    posts = posts.Where(t => searchTerms.Any(s => t.Body != null && t.Body.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                          || searchTerms.Any(s => t.AuthorDisplayName != null && t.AuthorDisplayName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
                }

            }

            await model.ReloadAsync(posts);

            model.AddForumMessage(message);
            return View("ShowTopic", model);
        }

        [HttpPost]
        [Route("forum/{slug}/{topicId}/{title}/")]
        [ForumAuthorize(ForumAccess.Post)]
        public async Task<IActionResult> AddPost(PostModel model, ForumMessage? message)
        {
            try
            {
                model.Topic = await _db.Topics
                         .Include(f => f.Author)
                         .Include(f => f.Forum)
                         .SingleOrDefaultAsync(f => f.Id == model.TopicId);

                if (model == null || model.Topic == null || model.Topic.Forum == null)
                    return NotFound();

                if (!model.Topic.Forum.Published)
                    return View("Offline", model);

                // check the topic - if its not up to snuff dont use it, just fire an error and pass the model on.
                if (model.Post == null)
                {
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "No topic was submitted.";
                    return await ShowTopic(model, null);
                }

                if (!model.Post.Body.IsSet())
                {
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "You have to enter a message.";
                    return await ShowTopic(model, null);
                }

                var user = await _userManager.GetUserAsync(User);
                model.Post.AuthorId = user.Id;
                model.Post.TopicId = model.Topic.Id;
                model.Post.Signature = user.ForumSignature;
                model.Post.PostedTime = DateTime.Now;
                model.Post.AuthorIp = HttpContext.Connection.RemoteIpAddress.ToString();
                model.Post.AuthorDisplayName = user.FullName;

                if (model.Topic.Forum.RequireTopicModeration)
                    model.Post.Approved = false;
                else
                    model.Post.Approved = true;

                _db.Posts.Add(model.Post);
                await _db.SaveChangesAsync();

                model.Topic.Forum.LastPosted = DateTime.Now;
                model.Topic.Forum.LastPostId = model.Post.Id;
                model.Topic.Forum.LastUserId = user.Id;
                model.Topic.Forum.LastUserName = user.UserName;
                model.Topic.Forum.LastUserDisplayName = user.FullName;

                model.Topic.LastPosted = DateTime.Now;
                model.Topic.LastPostId = model.Post.Id;
                model.Topic.LastUserId = user.Id;
                model.Topic.LastUserName = user.UserName;
                model.Topic.LastUserDisplayName = user.FullName;

                model.Topic.Forum.NumTopics = await _db.Topics.CountAsync(f => f.ForumId == model.Topic.ForumId);
                model.Topic.Forum.NumPosts = await _db.Posts.CountAsync(f => f.Topic.ForumId == model.Topic.ForumId);
                model.Topic.NumPosts = await _db.Posts.CountAsync(f => f.TopicId == model.TopicId);

                await _db.SaveChangesAsync();

                // Highlight the post we just created, so it is shown on load.#
                return RedirectToAction(nameof(ForumController.ShowTopic), new
                {
                    topicId = model.Topic.Id,
                    slug = model.Topic.Forum.Slug,
                    title = model.Topic.Title.ToSeoUrl(),
                    message = ForumMessage.Created,
                    highlight = model.Post.Id
                });
            }
            catch (Exception ex)
            {
                model.MessageType = AlertType.Danger;
                model.SaveMessage = "An error occured while adding your message: " + ex.Message;
                return await ShowTopic(model, null);
            }
        }

        #endregion

        #region "Editing" 

        [HttpPost]
        [Route("forum/{slug}/edit-post/{postId}/")]
        [ForumAuthorize(ForumAccess.Moderate)]
        public async Task<IActionResult> EditPost(long postId, string body)
        {
            var post = await _db.Posts
                 .Include(f => f.Author)
                 .Include(f => f.Topic).ThenInclude(t => t.Forum)
                 .SingleOrDefaultAsync(f => f.Id == postId);

            try
            {

                // check the topic - if its not up to snuff dont use it, just fire an error and pass the model on.
                if (post == null)
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.NoPostFound });

                post.Body = body;

                post.Edited = true;
                post.EditedById = _userManager.GetUserId(User);
                post.EditedTime = DateTime.Now;
                post.EditReason = "Modified by user.";

                post.Topic.LastEditedBy = _userManager.GetUserId(User);
                post.Topic.LastEditedOn = DateTime.Now;

                post.Topic.Forum.LastEditedBy = _userManager.GetUserId(User);
                post.Topic.Forum.LastEditedOn = DateTime.Now;

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(ForumController.ShowTopic),
                    new
                    {
                        topicId = post.Topic.Id,
                        slug = post.Topic.Forum.Slug,
                        title = post.Topic.Title.ToSeoUrl(),
                        highlight = post.Id,
                        message = ForumMessage.Saved
                    });

            }
            catch (Exception ex)
            {
                if (post != null)
                    return RedirectToAction(nameof(ForumController.ShowTopic),
                        new
                        {
                            topicId = post.Topic.Id,
                            slug = post.Topic.Forum.Slug,
                            title = post.Topic.Title.ToSeoUrl(),
                            highlight = post.Id,
                            message = ForumMessage.Error
                        });
                else
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.Error });
            }
        }

        [HttpPost]
        [Route("forum/{slug}/edit-topic/{topicId}/")]
        [ForumAuthorize(ForumAccess.Moderate)]
        public async Task<IActionResult> EditTopic(long topicId, string title, string description)
        {
            var topic = await _db.Topics
                .Include(t => t.Forum)
                .SingleOrDefaultAsync(f => f.Id == topicId);

            try
            {

                // check the topic - if its not up to snuff dont use it, just fire an error and pass the model on.
                if (topic == null)
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.NoTopicFound });

                topic.Title = title;
                topic.Description = description;

                topic.LastEditedBy = _userManager.GetUserId(User);
                topic.LastEditedOn = DateTime.Now;

                topic.Forum.LastEditedBy = _userManager.GetUserId(User);
                topic.Forum.LastEditedOn = DateTime.Now;

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(ForumController.ShowTopic),
                    new
                    {
                        topicId = topic.Id,
                        slug = topic.Forum.Slug,
                        title = topic.Title.ToSeoUrl(),
                        message = ForumMessage.Saved
                    });

            }
            catch (Exception ex)
            {
                if (topic != null)
                    return RedirectToAction(nameof(ForumController.ShowTopic),
                        new
                        {
                            topicId = topic.Id,
                            slug = topic.Forum.Slug,
                            title = topic.Title.ToSeoUrl(),
                            message = ForumMessage.Saved
                        });
                else
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.Error });
            }
        }

        #endregion

        #region "Actions"

        [Route("forum/{slug}/delete-post/{postId}/")]
        [ForumAuthorize(ForumAccess.Moderate)]
        public async Task<IActionResult> DeletePost(long postId)
        {
            var post = await _db.Posts
                 .Include(f => f.Author)
                 .Include(f => f.Topic).ThenInclude(t => t.Forum)
                 .SingleOrDefaultAsync(f => f.Id == postId);

            try
            {

                // check the topic - if its not up to snuff dont use it, just fire an error and pass the model on.
                if (post == null)
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.NoPostFound });

                _db.Entry(post).State = EntityState.Deleted;

                await _db.SaveChangesAsync();

                var topic = await _db.Topics
                         .Include(f => f.Forum)
                         .SingleOrDefaultAsync(f => f.Id == post.Topic.Id);

                topic.Forum.NumPosts = await _db.Posts.CountAsync(f => f.Topic.ForumId == topic.ForumId);
                topic.NumPosts = await _db.Posts.CountAsync(f => f.Id == topic.Id);

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(ForumController.ShowTopic),
                    new
                    {
                        topicId = post.Topic.Id,
                        slug = post.Topic.Forum.Slug,
                        title = post.Topic.Title.ToSeoUrl(),
                        message = ForumMessage.PostDeleted
                    });

            }
            catch (Exception ex)
            {
                if (post != null)
                    return RedirectToAction(nameof(ForumController.ShowTopic),
                        new
                        {
                            topicId = post.Topic.Id,
                            slug = post.Topic.Forum.Slug,
                            title = post.Topic.Title.ToSeoUrl(),
                            message = ForumMessage.Error
                        });
                else
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.Error });
            }
        }

        [Route("forum/{slug}/delete-topic/{topicId}/")]
        [ForumAuthorize(ForumAccess.Moderate)]
        public async Task<IActionResult> DeleteTopic(long topicId)
        {
            var topic = await _db.Topics
                .Include(t => t.Forum)
                .SingleOrDefaultAsync(f => f.Id == topicId);

            try
            {

                // check the topic - if its not up to snuff dont use it, just fire an error and pass the model on.
                if (topic == null)
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.NoTopicFound });

                _db.Entry(topic).State = EntityState.Deleted;

                await _db.SaveChangesAsync();

                var forum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == topic.ForumId);

                forum.NumPosts = await _db.Posts.CountAsync(f => f.Topic.ForumId == topic.ForumId);
                forum.NumTopics = await _db.Topics.CountAsync(f => f.ForumId == topic.ForumId);

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(ForumController.Topics),
                    new
                    {
                        slug = topic.Forum.Slug,
                        message = ForumMessage.TopicDeleted
                    });

            }
            catch (Exception ex)
            {
                if (topic != null)
                    return RedirectToAction(nameof(ForumController.Topics),
                        new
                        {
                            slug = topic.Forum.Slug,
                            message = ForumMessage.Error
                        });
                else
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.Error });
            }
        }

        [Authorize]
        [Route("forum/{slug}/report-post/{postId}/")]
        public async Task<IActionResult> ReportPost(long postId)
        {
            try
            {
                Post post = await _db.Posts
                         .Include(f => f.Author)
                         .Include(f => f.Topic).ThenInclude(t => t.Forum)
                         .SingleOrDefaultAsync(f => f.Id == postId);

                // check the topic - if its not up to snuff dont use it, just fire an error and pass the model on.
                if (post == null)
                    return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.NoPostFound });

                var reporter = await _userManager.GetUserAsync(User);

                MailObject message = new MailObject();
                message.PreHeader = _settings.ReplacePlaceholders("Abuse report");
                message.Subject = _settings.ReplacePlaceholders("Abuse report");
                message.AddParagraph(_settings.ReplacePlaceholders("The following post has been reported for abuse."));
                message = post.WriteToMessage(message);
                message.AddParagraph("The report was sent by: ");
                if (reporter == null)
                {
                    message.AddParagraph("Username - Anonymous User (Not logged in)");
                    message.AddParagraph("IP Address - ", HttpContext.Connection.RemoteIpAddress.ToString());
                }
                else
                {
                    message.AddParagraph(string.Format("Username - <strong>{0}</strong>", reporter.UserName));
                    message.AddParagraph(string.Format("Email - <strong>{0}</strong>", reporter.Email));
                    message.AddParagraph(string.Format("Display Name - <strong>{0}</strong>", reporter.DisplayName));
                    message.AddParagraph(string.Format("IP Address - <strong>{0}</strong>", HttpContext.Connection.RemoteIpAddress.ToString()));
                }
                message.AddDiv("<hr /><br />");
                message.AddParagraph("Click below to view the post on the forum:");
                var url = Url.AbsoluteAction("ShowTopic", "Forum", new { slug = post.Topic.Forum.Slug, title = post.Topic.Title.ToSeoUrl(), id = post.TopicId });
                message.AddCallToAction("View reported post", string.Format("{0}?highlight={1}", url, post.Id), "#f39c12", "center");

                if (_environment.IsDevelopment() || _environment.IsStaging())
                {
                    await _email.NotifyRoleAsync(message, "SuperUser", MailSettings.WarningTemplate);
                }
                else
                {
                    await _email.NotifyRoleAsync(message, "Moderator", MailSettings.WarningTemplate);
                }

                return RedirectToAction(nameof(ForumController.ShowTopic), new
                {
                    topicId = post.Topic.Id,
                    slug = post.Topic.Forum.Slug,
                    title = post.Topic.Title.ToSeoUrl(),
                    message = ForumMessage.PostReported
                });

            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(ForumController.Index), new { message = ForumMessage.Error });
            }
        }

        #endregion

        [Route("forums/unauthorized")]
        public IActionResult Unauthorized(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

    }
}


