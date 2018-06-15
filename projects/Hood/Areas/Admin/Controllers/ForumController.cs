using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.IO;
using Hood.Models;
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

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor,Manager")]
    public class ForumController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public ForumController()
            : base()
        {
        }

        [Route("admin/forums/manage/")]
        public async Task<IActionResult> Index(ForumModel model, EditorMessage? message)
        {
            IQueryable<Forum> forums = _db.Forums
                .Include(f => f.Author)
                .Include(f => f.Categories).ThenInclude(c => c.Category)
                .Include(f => f.Topics);

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
            model.AddEditorMessage(message);
            return View(model);
        }

        [Route("admin/forums/edit/{id}/")]
        public async Task<IActionResult> Edit(int id, EditorMessage? message)
        {
            var model = await LoadForum(id);

            if (model == null)
                return NotFound();

            model.Authors = await GetAuthorsAsync();
            model.AddEditorMessage(message);

            model.Subscriptions = await _auth.GetSubscriptionPlansAsync();
            model.Roles = _auth.GetAllRoles();

            return View(model);
        }

        [HttpPost()]
        [Route("admin/forums/edit/{id}/")]
        public async Task<ActionResult> Edit(Forum model)
        {
            try
            {
                model.LastEditedBy = _userManager.GetUserId(User);
                model.LastEditedOn = DateTime.Now;

                var author = await _userManager.FindByIdAsync(model.AuthorId);
                if (author != null)
                {
                    model.AuthorId = author.Id;
                    model.AuthorDisplayName = author.DisplayName;
                    model.AuthorName = author.FullName;
                }

                if (model.Slug.IsSet())
                {
                    if (!CheckSlug(model.Slug, model.Id))
                        throw new Exception("The slug is not valid, it already exists or is a reserved system word.");
                }
                else
                {
                    var generator = new KeyGenerator();
                    model.Slug = generator.UrlSlug();
                    while (!CheckSlug(model.Slug, model.Id))
                        model.Slug = generator.UrlSlug();
                }

                _db.Update(model);
                await _db.SaveChangesAsync();

                model = await LoadForum(model.Id, false);
                model.Authors = await GetAuthorsAsync();

                model.SaveMessage = "Forum saved.";
                model.MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                var reload = await _db.Forums.Include(f => f.Categories).SingleOrDefaultAsync(f => f.Id == model.Id);
                model.Categories = reload.Categories;
                model.Authors = await GetAuthorsAsync();

                model.SaveMessage = "There was a problem saving: " + ex.Message;
                model.MessageType = AlertType.Danger;
            }

            model.Subscriptions = await _auth.GetSubscriptionPlansAsync();
            model.Roles = _auth.GetAllRoles();

            return View(model);
        }

        [Route("admin/forums/create/")]
        public IActionResult Create()
        {
            Forum model = new Forum();
            return View(model);
        }

        [HttpPost]
        [Route("admin/forums/create/")]
        public async Task<Response> Create(Forum model)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);

                // Generate temp slug
                var generator = new KeyGenerator();
                model.Slug = generator.UrlSlug();
                while (!CheckSlug(model.Slug))
                    model.Slug = generator.UrlSlug();

                model.AuthorId = user.Id;
                model.AuthorDisplayName = user.DisplayName;
                model.AuthorName = user.FullName;

                model.CreatedBy = user.UserName;
                model.CreatedOn = DateTime.Now;
                model.LastEditedBy = user.UserName;
                model.LastEditedOn = DateTime.Now;

                model.ShareCount = 0;
                model.Views = 0;

                _db.Forums.Add(model);
                await _db.SaveChangesAsync();

                var response = new Response(true, "Created successfully.");
                response.Url = Url.Action("Edit", new { id = model.Id, message = EditorMessage.Created });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/forums/categories/")]
        public IActionResult CategoryList()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/forums/categories/add/")]
        public async Task<IActionResult> AddCategory(int forumId, int categoryId)
        {
            try
            {
                var model = await LoadForum(forumId, false);

                if (model.IsInCategory(categoryId))
                    return Json(new { Success = true });

                var category = await _db.ForumCategories.SingleOrDefaultAsync(c => c.Id == categoryId);
                if (category == null)
                    throw new Exception("The category does not exist.");

                model.Categories.Add(new ForumCategoryJoin() { CategoryId = category.Id, ForumId = model.Id });

                _db.Update(model);
                await _db.SaveChangesAsync();
                _forumCategoryCache.ResetCache();

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [HttpPost]
        [Route("admin/forums/categories/remove/")]
        public async Task<IActionResult> RemoveCategory(int forumId, int categoryId)
        {
            try
            {
                var model = await LoadForum(forumId, false);

                if (!model.IsInCategory(categoryId))
                    return Json(new { Success = true });

                var cat = model.Categories.SingleOrDefault(c => c.CategoryId == categoryId);

                if (cat == null)
                    return Json(new { Success = true });

                model.Categories.Remove(cat);

                _db.Update(model);
                await _db.SaveChangesAsync();
                _forumCategoryCache.ResetCache();

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [HttpPost]
        [Route("admin/forums/categories/create/")]
        public async Task<IActionResult> CreateCategory(ForumCategory model)
        {
            try
            {
                // User must have an organisation.
                AccountInfo account = HttpContext.GetAccountInfo();

                // check if category is on club already
                if (!model.DisplayName.IsSet())
                    throw new Exception("You need to enter a category!");


                // check if it exists in the db, if not add it. 
                // Ensure it is in title case.
                model.DisplayName = model.DisplayName.Trim().ToTitleCase();
                model.Slug = model.DisplayName.ToSeoUrl();
                int counter = 1;
                while (await _db.ForumCategories.CountAsync(cc => cc.Slug == model.Slug) > 0)
                {
                    model.Slug = model.DisplayName.ToSeoUrl() + "-" + counter;
                    counter++;
                }

                _db.ForumCategories.Add(model);
                await _db.SaveChangesAsync();
                _forumCategoryCache.ResetCache();

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [Route("admin/forums/categories/edit/{id}/")]
        public async Task<IActionResult> EditCategory(int id)
        {
            var model = await _db.ForumCategories.FirstOrDefaultAsync(c => c.Id == id);
            model.Categories = _forumCategoryCache.TopLevel();
            return View(model);
        }

        [Route("admin/forums/categories/save/")]
        public async Task<Response> EditCategory(ForumCategory model)
        {
            try
            {
                if (model.ParentCategoryId.HasValue)
                {
                    if (model.ParentCategoryId == model.Id)
                        throw new Exception("You cannot set the parent to be the same category!");

                    var thisAndChildren = _forumCategoryCache.GetThisAndChildren(model.Id);
                    if (thisAndChildren.Select(c => c.Id).ToList().Contains(model.ParentCategoryId.Value))
                        throw new Exception("You cannot set the parent to be a child of this category!");
                }

                _db.Update(model);
                await _db.SaveChangesAsync();
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/forums/categories/delete/{id}/")]
        public async Task<Response> DeleteCategory(int id)
        {
            try
            {
                var category = await _db.ForumCategories.FirstOrDefaultAsync(c => c.Id == id);
                _db.Entry(category).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
                _forumCategoryCache.ResetCache();
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response("Have you made sure this has no sub-categories attached to it, you cannot delete a category until you remove all the sub-categories from it");
            }
        }


        [Route("admin/forums/publish/{id}/")]
        [HttpPost()]
        public async Task<Response> Publish(int id)
        {
            try
            {
                var model = await LoadForum(id, false);

                model.Published = true;

                _db.Forums.Update(model);
                await _db.SaveChangesAsync();

                var response = new Response(true, "Published successfully.");
                response.Url = Url.Action("Index", new { id = model.Id, message = EditorMessage.Published });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }
        [Route("admin/forums/archive/{id}")]
        [HttpPost()]
        public async Task<Response> Archive(int id)
        {
            try
            {
                var model = await LoadForum(id, false);

                model.Published = false;

                _db.Forums.Update(model);
                await _db.SaveChangesAsync();

                var response = new Response(true, "Archived successfully.");
                response.Url = Url.Action("Index", new { id = model.Id, message = EditorMessage.Published });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [HttpPost()]
        [Route("admin/forums/delete/{id}")]
        public async Task<Response> Delete(int id)
        {
            try
            {
                var model = await LoadForum(id, false);

                _db.Entry(model).State = EntityState.Deleted;
                _db.SaveChanges();

                var response = new Response(true, "Deleted!");
                response.Url = Url.Action("Index", new { message = EditorMessage.Deleted });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/forums/getfeaturedimage/{id}")]
        public async Task<IMediaObject> GetFeaturedImage(int id)
        {
            try
            {
                var model = await LoadForum(id, false);

                if (model != null && model.FeaturedImage != null)
                    return model.FeaturedImage;
                else
                    throw new Exception("No featured image found");
            }
            catch (Exception)
            {
                return ForumMedia.Blank;
            }
        }

        [Route("admin/forums/getsharerimage/{id}")]
        public async Task<IMediaObject> GetSharerImage(int id)
        {
            try
            {
                var model = await LoadForum(id, false);

                if (model != null && model.ShareImage != null)
                    return model.ShareImage;
                else
                    throw new Exception("No sharer image found");
            }
            catch (Exception)
            {
                return ForumMedia.Blank;
            }
        }

        [Route("admin/forums/clearimage/{id}")]
        public async Task<Response> ClearImage(int id)
        {
            try
            {
                var model = await LoadForum(id, false);

                model.FeaturedImage = null;

                _db.Update(model);
                await _db.SaveChangesAsync();
                var response = new Response(true, "The image has been cleared!");
                response.Url = Url.Action("Edit", new { id = id, message = EditorMessage.MediaRemoved });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/forums/clearshareimage/{id}")]
        public async Task<Response> ClearShareImage(int id)
        {
            try
            {
                var model = await LoadForum(id, false);

                model.ShareImage = null;

                _db.Update(model);
                await _db.SaveChangesAsync();
                var response = new Response(true, "The image has been cleared!");
                response.Url = Url.Action("Edit", new { id = id, message = EditorMessage.MediaRemoved });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/forums/categories/suggestions/{type}/")]
        public IActionResult CategorySuggestions(string type)
        {
            var suggestions = _forumCategoryCache.GetSuggestions().Select(c => new { id = c.Id, displayName = c.DisplayName, slug = c.Slug });
            return Json(suggestions.ToArray());
        }

        private bool CheckSlug(string slug, int? id = null)
        {
            if (id.HasValue)
                return _db.Forums.SingleOrDefault(c => c.Slug == slug && c.Id != id) == null;
            return _db.Forums.SingleOrDefault(c => c.Slug == slug) == null;
        }

        private async Task<List<ApplicationUser>> GetAuthorsAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var editors = await _userManager.GetUsersInRoleAsync("Editor");
            var moderators = await _userManager.GetUsersInRoleAsync("Moderator");
            return editors.Concat(admins).Concat(moderators).Distinct().OrderBy(u => u.FirstName).ThenBy(u => u.Email).ToList();
        }

        private async Task<Forum> LoadForum(int id, bool includeTopics = false)
        {
            if (includeTopics)
                return await _db.Forums
                    .Include(f => f.Author)
                    .Include(f => f.Categories)
                    .Include(f => f.Topics)
                    .SingleOrDefaultAsync(f => f.Id == id);
            else
                return await _db.Forums
                    .Include(f => f.Author)
                    .Include(f => f.Categories)
                    .SingleOrDefaultAsync(f => f.Id == id);
        }
    }
}


