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
    public class ForumController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HoodDbContext _db;
        private readonly ForumCategoryCache _categories;
        private readonly ISettingsRepository _settings;
        private readonly IHostingEnvironment _env;
        private readonly IMediaManager<MediaObject> _media;
        private readonly IAccountRepository _auth;

        public ForumController(
            IAccountRepository auth,
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
            _env = env;
            _categories = categories;
        }

        [Route("admin/forums/manage/")]
        public async Task<IActionResult> Index(ForumModel model, EditorMessage? message)
        {
            IQueryable<Forum> forums = _db.Forums.Include(f => f.Categories).Include(f => f.Topics);

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

            await model.ReloadAsync(forums, model.PageIndex, model.PageSize);
            model.AddEditorMessage(message);
            return View(model);
        }

        [Route("admin/forums/edit/{id}/")]
        public async Task<IActionResult> Edit(int id, EditorMessage? message)
        {
            var model = await _db.Forums
                .Include(f => f.Categories)
                .Include(f => f.Topics)
                .SingleOrDefaultAsync(f => f.Id == id);
            if (model == null)
                return NotFound();
            model.AddEditorMessage(message);
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

                return View(model);

            }
            catch (Exception ex)
            {
                model.SaveMessage = "There was a problem saving: " + ex.Message;
                model.MessageType = AlertType.Danger;
                return View(model);
            }
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
                model.Body = "";

                model.CreatedBy = user.UserName;
                model.CreatedOn = DateTime.Now;
                model.LastEditedBy = user.UserName;
                model.LastEditedOn = DateTime.Now;

                model.Public = true;
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
        [Route("admin/forum/categories/add/")]
        public async Task<IActionResult> AddCategory(int forumId, int categoryId)
        {
            try
            {
                var model = await _db.Forums
                    .Include(f => f.Categories)
                    .Include(f => f.Topics)
                    .SingleOrDefaultAsync(f => f.Id == forumId);

                if (model.IsInCategory(categoryId)) // Content is already in!
                    return Json(new { Success = true });

                var category = await _db.ForumCategories.SingleOrDefaultAsync(c => c.Id == categoryId);
                if (category == null)
                    throw new Exception("The category does not exist.");

                model.Categories.Add(new ForumCategoryJoin() { CategoryId = category.Id, ForumId = model.Id });

                _db.Update(model);
                await _db.SaveChangesAsync();

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [Route("admin/forums/categories/remove/")]
        public async Task<IActionResult> RemoveCategory(int forumId, int categoryId)
        {
            try
            {
                var model = await _db.Forums
                    .Include(f => f.Categories)
                    .Include(f => f.Topics)
                    .SingleOrDefaultAsync(f => f.Id == forumId);

                if (!model.IsInCategory(categoryId)) // Content is already in!
                    return Json(new { Success = true });

                var cat = model.Categories.SingleOrDefault(c => c.CategoryId == categoryId);

                if (cat == null)// Content is already out!
                    return Json(new { Success = true });

                model.Categories.Remove(cat);

                _db.Update(model);
                await _db.SaveChangesAsync();

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [HttpPost]
        [Route("admin/forums/categories/add/")]
        public async Task<IActionResult> AddCategory(ForumCategory model)
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
                while (await _db.ContentCategories.CountAsync(cc => cc.Slug == model.Slug) > 0)
                {
                    model.Slug = model.DisplayName.ToSeoUrl() + "-" + counter;
                    counter++;
                }
                var exists = _db.ForumCategories.SingleOrDefault(t => t.DisplayName == model.DisplayName && t.ParentCategoryId == model.ParentCategoryId);
                if (exists == null)
                {
                    _db.ForumCategories.Add(model);
                    await _db.SaveChangesAsync();
                }

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
            model.Categories = _categories.TopLevel();
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

                    var thisAndChildren = _categories.GetThisAndChildren(model.Id);
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
                var category = await _db.ContentCategories.FirstOrDefaultAsync(c => c.Id == id);
                _db.Entry(category).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
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
                var model = await _db.Forums.FindAsync(id);
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
                var model = await _db.Forums.FindAsync(id);
                model.Published = true;
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
                var model = await _db.Forums
                    .Include(f => f.Categories)
                    .Include(f => f.Topics)
                    .SingleOrDefaultAsync(f => f.Id == id);
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
                var model = await _db.Forums
                                .SingleOrDefaultAsync(f => f.Id == id);
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
                var model = await _db.Forums
                                .SingleOrDefaultAsync(f => f.Id == id);
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
                var model = await _db.Forums
                                .SingleOrDefaultAsync(f => f.Id == id);

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

        [Route("admin/forums/clearsharerimage/{id}")]
        public async Task<Response> ClearSharerImage(int id)
        {
            try
            {
                var model = await _db.Forums
                                .SingleOrDefaultAsync(f => f.Id == id);

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
            var suggestions = _categories.GetSuggestions().Select(c => new { id = c.Id, displayName = c.DisplayName, slug = c.Slug });
            return Json(suggestions.ToArray());
        }

        public bool CheckSlug(string slug, int? id = null)
        {
            if (id.HasValue)
                return _db.Forums.SingleOrDefault(c => c.Slug == slug && c.Id != id) == null;
            return _db.Forums.SingleOrDefault(c => c.Slug == slug) == null;
        }

    }
}


