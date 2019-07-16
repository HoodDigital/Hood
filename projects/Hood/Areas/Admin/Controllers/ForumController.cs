using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class ForumController : BaseController
    {
        public ForumController()
            : base()
        {
        }

        [Route("admin/forums/manage/")]
        public async Task<IActionResult> Index(ForumModel model)
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
            return View(model);
        }

        [Route("admin/forums/edit/{id}/")]
        public async Task<IActionResult> Edit(int id)
        {
            Forum model = await LoadForum(id);

            if (model == null)
            {
                return NotFound();
            }

            model.Authors = await GetAuthorsAsync();

            SubscriptionSearchModel subs = await _account.GetSubscriptionPlansAsync(new SubscriptionSearchModel() { PageSize = int.MaxValue });
            model.Subscriptions = subs.List;

            model.Roles = await _account.GetAllRolesAsync();

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

                ApplicationUser author = await _userManager.FindByIdAsync(model.AuthorId);
                if (author != null)
                {
                    model.AuthorId = author.Id;
                    model.AuthorDisplayName = author.ToDisplayName();
                    model.AuthorName = author.ToFullName();
                }

                if (model.Slug.IsSet())
                {
                    if (!CheckSlug(model.Slug, model.Id))
                    {
                        throw new Exception("The slug is not valid, it already exists or is a reserved system word.");
                    }
                }
                else
                {
                    KeyGenerator generator = new KeyGenerator();
                    model.Slug = generator.UrlSlug();
                    while (!CheckSlug(model.Slug, model.Id))
                    {
                        model.Slug = generator.UrlSlug();
                    }
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
                Forum reload = await _db.Forums.Include(f => f.Categories).SingleOrDefaultAsync(f => f.Id == model.Id);
                model.Categories = reload.Categories;
                model.Authors = await GetAuthorsAsync();

                model.SaveMessage = "There was a problem saving: " + ex.Message;
                model.MessageType = AlertType.Danger;

                await _logService.AddExceptionAsync<ForumController>(SaveMessage, ex);
            }

            SubscriptionSearchModel subs = await _account.GetSubscriptionPlansAsync(new SubscriptionSearchModel() { PageSize = int.MaxValue });
            model.Subscriptions = subs.List;

            model.Roles = await _account.GetAllRolesAsync();

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
                KeyGenerator generator = new KeyGenerator();
                model.Slug = generator.UrlSlug();
                while (!CheckSlug(model.Slug))
                {
                    model.Slug = generator.UrlSlug();
                }

                model.AuthorId = user.Id;
                model.AuthorDisplayName = user.ToDisplayName();
                model.AuthorName = user.ToFullName();

                IList<string> roles = await _userManager.GetRolesAsync(user);
                model.AuthorRoles = string.Join(",", roles);

                model.CreatedBy = user.UserName;
                model.CreatedOn = DateTime.Now;
                model.LastEditedBy = user.UserName;
                model.LastEditedOn = DateTime.Now;

                model.ShareCount = 0;
                model.Views = 0;

                _db.Forums.Add(model);
                await _db.SaveChangesAsync();

#warning TODO: Handle response in JS.
                return new Response(true, "Created successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error creating a forum.", ex);
            }
        }

        [Route("admin/forums/categories/")]
        public IActionResult CategoryList()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/forums/categories/add/")]
        public async Task<Response> AddCategory(int forumId, int categoryId)
        {
            try
            {
                Forum model = await LoadForum(forumId, false);

                if (model.IsInCategory(categoryId))
                {
                    return new Response(true, "Forum already is in that category.");
                }

                ForumCategory category = await _db.ForumCategories.SingleOrDefaultAsync(c => c.Id == categoryId);
                if (category == null)
                {
                    throw new Exception("The category does not exist.");
                }

                model.Categories.Add(new ForumCategoryJoin() { CategoryId = category.Id, ForumId = model.Id });

                _db.Update(model);
                await _db.SaveChangesAsync();
                _forumCategoryCache.ResetCache();

#warning TODO: Handle response in JS.
                return new Response(true, "Added the category to the forum.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error adding a forum category", ex);
            }
        }

        [HttpPost]
        [Route("admin/forums/categories/remove/")]
        public async Task<Response> RemoveCategory(int forumId, int categoryId)
        {
            try
            {
                Forum model = await LoadForum(forumId, false);

                if (!model.IsInCategory(categoryId))
                {
                    return new Response(true, "Forum is not in that category.");
                }

                ForumCategoryJoin cat = model.Categories.SingleOrDefault(c => c.CategoryId == categoryId);

                if (cat == null)
                {
                    throw new Exception("The category does not exist.");
                }

                model.Categories.Remove(cat);

                _db.Update(model);
                await _db.SaveChangesAsync();
                _forumCategoryCache.ResetCache();

#warning TODO: Handle response in JS.
                return new Response(true, "Removed the category from the forum.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error removing a forum category.", ex);
            }
        }

        [HttpPost]
        [Route("admin/forums/categories/create/")]
        public async Task<Response> CreateCategory(ForumCategory model)
        {
            try
            {
                // User must have an organisation.
                // check if category is on club already
                if (!model.DisplayName.IsSet())
                {
                    throw new Exception("You need to enter a category!");
                }


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

#warning TODO: Handle response in JS.
                return new Response(true, "The category was created successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error removing a forum category.", ex);
            }
        }

        [Route("admin/forums/categories/edit/{id}/")]
        public async Task<IActionResult> EditCategory(int id)
        {
            ForumCategory model = await _db.ForumCategories.FirstOrDefaultAsync(c => c.Id == id);
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
                    {
                        throw new Exception("You cannot set the parent to be the same category!");
                    }

                    IEnumerable<ForumCategory> thisAndChildren = _forumCategoryCache.GetThisAndChildren(model.Id);
                    if (thisAndChildren.Select(c => c.Id).ToList().Contains(model.ParentCategoryId.Value))
                    {
                        throw new Exception("You cannot set the parent to be a child of this category!");
                    }
                }

                _db.Update(model);
                await _db.SaveChangesAsync();
                return new Response(true, $"The category has been saved.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error updating a forum category.", ex);
            }
        }

        [Route("admin/forums/categories/delete/{id}/")]
        public async Task<Response> DeleteCategory(int id)
        {
            try
            {
                ForumCategory category = await _db.ForumCategories.FirstOrDefaultAsync(c => c.Id == id);
                _db.Entry(category).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
                _forumCategoryCache.ResetCache();
                return new Response(true, $"The category has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error deleting a forum category, did you make sure it was empty first?", ex);
            }
        }


        [Route("admin/forums/publish/{id}/")]
        [HttpPost()]
        public async Task<Response> Publish(int id)
        {
            try
            {
                Forum model = await LoadForum(id, false);

                model.Published = true;

                _db.Forums.Update(model);
                await _db.SaveChangesAsync();


#warning TODO: Handle response in JS.
                return new Response(true, "Published successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error publishing a forum.", ex);
            }
        }
        [Route("admin/forums/archive/{id}")]
        [HttpPost()]
        public async Task<Response> Archive(int id)
        {
            try
            {
                Forum model = await LoadForum(id, false);

                model.Published = false;

                _db.Forums.Update(model);
                await _db.SaveChangesAsync();


#warning TODO: Handle response in JS.
                return new Response(true, "Archived successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error archiving a forum.", ex);
            }
        }

        [HttpPost()]
        [Route("admin/forums/delete/{id}")]
        public async Task<Response> Delete(int id)
        {
            try
            {
                Forum model = await LoadForum(id, false);

                _db.Entry(model).State = EntityState.Deleted;
                _db.SaveChanges();

#warning TODO: Handle response in JS.
                return new Response(true, "Deleted!");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ForumController>($"Error deleting a forum.", ex);
            }
        }

        [Route("admin/forums/categories/suggestions/")]
        public IActionResult CategorySuggestions()
        {
            var suggestions = _forumCategoryCache.GetSuggestions().Select(c => new { id = c.Id, displayName = c.DisplayName, slug = c.Slug });
            return Json(suggestions.ToArray());
        }

        private bool CheckSlug(string slug, int? id = null)
        {
            if (id.HasValue)
            {
                return _db.Forums.SingleOrDefault(c => c.Slug == slug && c.Id != id) == null;
            }

            return _db.Forums.SingleOrDefault(c => c.Slug == slug) == null;
        }

        private async Task<List<ApplicationUser>> GetAuthorsAsync()
        {
            IList<ApplicationUser> admins = await _userManager.GetUsersInRoleAsync("Admin");
            IList<ApplicationUser> editors = await _userManager.GetUsersInRoleAsync("Editor");
            IList<ApplicationUser> moderators = await _userManager.GetUsersInRoleAsync("Moderator");
            return editors.Concat(admins).Concat(moderators).Distinct().OrderBy(u => u.FirstName).ThenBy(u => u.Email).ToList();
        }

        private async Task<Forum> LoadForum(int id, bool includeTopics = false)
        {
            if (includeTopics)
            {
                return await _db.Forums
                    .Include(f => f.Author)
                    .Include(f => f.Categories)
                    .Include(f => f.Topics)
                    .SingleOrDefaultAsync(f => f.Id == id);
            }
            else
            {
                return await _db.Forums
                    .Include(f => f.Author)
                    .Include(f => f.Categories)
                    .SingleOrDefaultAsync(f => f.Id == id);
            }
        }
    }
}


