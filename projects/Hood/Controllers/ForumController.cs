using Hood.Enums;
using Hood.Extensions;
using Hood.Filters;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public class ForumController : BaseController
    {
        public ForumController()
            : base()
        { }

        #region "Forum Views"

        [Route("forums/")]
        [ForumAuthorize(ForumAccess.View)]
        public async Task<IActionResult> Index(ForumModel model)
        {
            IQueryable<Forum> forums = _db.Forums
                .Include(f => f.Author)
                .Include(f => f.Categories).ThenInclude(c => c.Category)
                .Where(f => f.Published);

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
            return View(model);
        }

        [Route("forum/{slug}")]
        [ForumAuthorize(ForumAccess.View)]
        public async Task<IActionResult> Topics(TopicModel model)
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
                model.Topic.AuthorDisplayName = user.ToDisplayName();
                model.Topic.AuthorName = user.ToFullName();
                model.Topic.ForumId = model.Forum.Id;
                model.Topic.CreatedOn = DateTime.Now;

                if (model.Forum.RequireTopicModeration)
                    model.Topic.Approved = false;
                else
                    model.Topic.Approved = true;

                var roles = await _userManager.GetRolesAsync(user);
                model.Topic.AuthorRoles = string.Join(",", roles);

                _db.Topics.Add(model.Topic);
                await _db.SaveChangesAsync();

                model.Forum.NumTopics = await _db.Topics.CountAsync(f => f.ForumId == model.Topic.ForumId);
                await _db.SaveChangesAsync();

                SaveMessage = $"The topic was added successfully.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                MessageType = AlertType.Danger;
                SaveMessage = "An error occured while adding your message: " + ex.Message;
            }

            return await Topics(model);
        }

        [Route("forum/{slug}/{topicId}/{title}/")]
        [ForumAuthorize(ForumAccess.View)]
        public async Task<IActionResult> ShowTopic(PostModel model)
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

            return View("ShowTopic", model);
        }

        [HttpPost]
        [Route("forum/{slug}/{topicId}/{title}/")]
        [ForumAuthorize(ForumAccess.Post)]
        public async Task<IActionResult> AddPost(PostModel model)
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
                    MessageType = AlertType.Danger;
                    SaveMessage = "No topic was submitted.";
                    return await ShowTopic(model);
                }

                if (!model.Post.Body.IsSet())
                {
                    MessageType = AlertType.Danger;
                    SaveMessage = "You have to enter a message.";
                    return await ShowTopic(model);
                }

                var user = await _userManager.GetUserAsync(User);
                model.Post.AuthorId = user.Id;
                model.Post.TopicId = model.Topic.Id;
                model.Post.Signature = user.ForumSignature;
                model.Post.PostedTime = DateTime.Now;
                model.Post.AuthorIp = HttpContext.Connection.RemoteIpAddress.ToString();
                model.Post.AuthorDisplayName = user.ToFullName();

                if (model.Topic.Forum.RequireTopicModeration)
                    model.Post.Approved = false;
                else
                    model.Post.Approved = true;

                var roles = await _userManager.GetRolesAsync(user);
                model.Post.AuthorRoles = string.Join(",", roles);

                _db.Posts.Add(model.Post);
                await _db.SaveChangesAsync();

                model.Topic.Forum.LastPosted = DateTime.Now;
                model.Topic.Forum.LastPostId = model.Post.Id;
                model.Topic.Forum.LastUserId = user.Id;
                model.Topic.Forum.LastUserName = user.UserName;
                model.Topic.Forum.LastUserDisplayName = user.ToDisplayName();

                model.Topic.LastPosted = DateTime.Now;
                model.Topic.LastPostId = model.Post.Id;
                model.Topic.LastUserId = user.Id;
                model.Topic.LastUserName = user.UserName;
                model.Topic.LastUserDisplayName = user.ToDisplayName();

                model.Topic.Forum.NumTopics = await _db.Topics.CountAsync(f => f.ForumId == model.Topic.ForumId);
                model.Topic.Forum.NumPosts = await _db.Posts.CountAsync(f => f.Topic.ForumId == model.Topic.ForumId);
                model.Topic.NumPosts = await _db.Posts.CountAsync(f => f.TopicId == model.TopicId);

                await _db.SaveChangesAsync();

                SaveMessage = $"Your post was submitted successfully.";
                MessageType = AlertType.Success;

                // Highlight the post we just created, so it is shown on load.#
                return RedirectToAction(nameof(ForumController.ShowTopic), new
                {
                    topicId = model.Topic.Id,
                    slug = model.Topic.Forum.Slug,
                    title = model.Topic.Title.ToSeoUrl(),
                    highlight = model.Post.Id
                });
            }
            catch (Exception ex)
            {
                MessageType = AlertType.Danger;
                SaveMessage = "An error occured while adding your message: " + ex.Message;
                return await ShowTopic(model);
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
                    throw new Exception("Post not found.");

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

                SaveMessage = $"The post has been updated.";
                MessageType = AlertType.Success;

                return RedirectToAction(nameof(ForumController.ShowTopic),
                    new
                    {
                        topicId = post.Topic.Id,
                        slug = post.Topic.Forum.Slug,
                        title = post.Topic.Title.ToSeoUrl(),
                        highlight = post.Id
                    });

            }
            catch (Exception ex)
            {
                SaveMessage = $"Error occurred while editing a {nameof(Post)}.";
                if (post != null)
                {
                    await _logService.AddExceptionAsync<ForumController>(SaveMessage, post, ex);
                    return RedirectToAction(nameof(ForumController.ShowTopic),
                        new
                        {
                            topicId = post.Topic.Id,
                            slug = post.Topic.Forum.Slug,
                            title = post.Topic.Title.ToSeoUrl(),
                            highlight = post.Id
                        });
                }
                else
                {
                    await _logService.AddExceptionAsync<ForumController>(SaveMessage, ex);
                    return RedirectToAction(nameof(ForumController.Index));
                }
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
                    throw new Exception("Topic not found.");

                topic.Title = title;
                topic.Description = description;

                topic.LastEditedBy = _userManager.GetUserId(User);
                topic.LastEditedOn = DateTime.Now;

                topic.Forum.LastEditedBy = _userManager.GetUserId(User);
                topic.Forum.LastEditedOn = DateTime.Now;

                await _db.SaveChangesAsync();

                SaveMessage = $"The topic has been updated.";
                MessageType = AlertType.Success;

                return RedirectToAction(nameof(ForumController.ShowTopic),
                    new
                    {
                        topicId = topic.Id,
                        slug = topic.Forum.Slug,
                        title = topic.Title.ToSeoUrl()
                    });

            }
            catch (Exception ex)
            {
                SaveMessage = $"Error editing a {nameof(Topic)}.";
                if (topic != null)
                {
                    await _logService.AddExceptionAsync<ForumController>(SaveMessage, topic, ex);
                    MessageType = AlertType.Danger;
                    return RedirectToAction(nameof(ForumController.ShowTopic),
                        new
                        {
                            topicId = topic.Id,
                            slug = topic.Forum.Slug,
                            title = topic.Title.ToSeoUrl()
                        });
                }
                else
                {
                    await _logService.AddExceptionAsync<ForumController>(SaveMessage, ex);
                    return RedirectToAction(nameof(ForumController.Index));
                }
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
                    throw new Exception("Post not found.");

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
                        title = post.Topic.Title.ToSeoUrl()
                    });

            }
            catch (Exception ex)
            {
                SaveMessage = $"Error occurred while editing a {nameof(Post)}.";
                MessageType = AlertType.Danger;
                if (post != null)
                {
                    await _logService.AddExceptionAsync<ForumController>(SaveMessage, post, ex);
                    return RedirectToAction(nameof(ForumController.ShowTopic),
                        new
                        {
                            topicId = post.Topic.Id,
                            slug = post.Topic.Forum.Slug,
                            title = post.Topic.Title.ToSeoUrl()
                        });
                }
                else
                {
                    await _logService.AddExceptionAsync<ForumController>(SaveMessage, ex);
                    return RedirectToAction(nameof(ForumController.Index));
                }
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
                    throw new Exception("Topic not found.");

                _db.Entry(topic).State = EntityState.Deleted;

                await _db.SaveChangesAsync();

                var forum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == topic.ForumId);

                forum.NumPosts = await _db.Posts.CountAsync(f => f.Topic.ForumId == topic.ForumId);
                forum.NumTopics = await _db.Topics.CountAsync(f => f.ForumId == topic.ForumId);

                await _db.SaveChangesAsync();

                SaveMessage = $"The topic has been deleted.";
                MessageType = AlertType.Success;

                return RedirectToAction(nameof(ForumController.Topics),
                    new
                    {
                        slug = topic.Forum.Slug
                    });

            }
            catch (Exception ex)
            {
                SaveMessage = $"Error occurred while editing a {nameof(Topic)}.";
                MessageType = AlertType.Danger;
                if (topic != null)
                {
                    await _logService.AddExceptionAsync<ForumController>(SaveMessage, topic, ex);
                    return RedirectToAction(nameof(ForumController.Topics),
                        new
                        {
                            slug = topic.Forum.Slug
                        });
                }
                else
                {
                    await _logService.AddExceptionAsync<ForumController>(SaveMessage, ex);
                    return RedirectToAction(nameof(ForumController.Index));
                }
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
                    throw new Exception("Post not found.");

                var reporter = await _userManager.GetUserAsync(User);

                MailObject message = new MailObject
                {
                    PreHeader = "Abuse report",
                    Subject = "Abuse report"
                };
                message.AddParagraph("The following post has been reported for abuse.");
                message = post.WriteToMailObject(message);
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
                    message.AddParagraph(string.Format("Display Name - <strong>{0}</strong>", reporter.ToDisplayName()));
                    message.AddParagraph(string.Format("IP Address - <strong>{0}</strong>", HttpContext.Connection.RemoteIpAddress.ToString()));
                }
                message.AddDiv("<hr /><br />");
                message.AddParagraph("Click below to view the post on the forum:");
                var url = Url.AbsoluteAction("ShowTopic", "Forum", new { slug = post.Topic.Forum.Slug, title = post.Topic.Title.ToSeoUrl(), id = post.TopicId });
                message.AddCallToAction("View reported post", string.Format("{0}?highlight={1}", url, post.Id), "#f39c12", "center");
                message.Template = MailSettings.WarningTemplate;

                await _emailSender.NotifyRoleAsync(message, "SuperUser");
                await _emailSender.NotifyRoleAsync(message, "Moderator");

                SaveMessage = $"Thank you. The post has been reported to our moderators.";
                MessageType = AlertType.Success;

                return RedirectToAction(nameof(ForumController.ShowTopic), new
                {
                    topicId = post.Topic.Id,
                    slug = post.Topic.Forum.Slug,
                    title = post.Topic.Title.ToSeoUrl()
                });

            }
            catch (Exception ex)
            {
                SaveMessage = $"Errorreporting a {nameof(Post)} with Id: {postId}.";
                await _logService.AddExceptionAsync<ForumController>(SaveMessage, ex);
                return RedirectToAction(nameof(ForumController.Index));
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


