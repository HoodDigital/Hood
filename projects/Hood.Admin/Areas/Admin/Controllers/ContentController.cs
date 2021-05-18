using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class ContentController : BaseController
    {
        public ContentController()
            : base()
        {
        }

        [Route("admin/content/manage/{type?}/")]
        public async Task<IActionResult> Index(ContentModel model) => await List(model, "Index");

        [Route("admin/content/list/{type?}/")]
        public async Task<IActionResult> List(ContentModel model, string viewName = "_List_Content")
        {
            model = await _content.GetContentAsync(model);
            model.ContentType = Engine.Settings.Content.GetContentType(model.Type);
            return View(viewName, model);
        }

        #region Edit
        [Route("admin/content/{id}/edit/")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _content.GetContentByIdAsync(id, true, false);
            model = await GetEditorModel(model);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost()]
        [Route("admin/content/{id}/edit/")]
        public async Task<ActionResult> Edit(Content model)
        {
            try
            {
                model.LastEditedBy = User.Identity.Name;
                model.LastEditedOn = DateTime.Now;

                if (model.Slug.IsSet())
                {
                    if (await _content.SlugExists(model.Slug, model.Id))
                    {
                        model.Type = Engine.Settings.Content.GetContentType(model.ContentType);
                        if (model.Type.BaseName == "Page")
                        {
                            throw new Exception("The slug is not valid, it already exists or is a reserved system word.");
                        }
                        else
                        {
                            await GenerateNewSlug(model);
                        }
                    }
                }
                else
                {
                    await GenerateNewSlug(model);
                }

                // Save and reload to deal with metas.
                await _content.UpdateAsync(model);
                // reload
                model = await _content.GetContentByIdAsync(model.Id, true, false);
                model = await GetEditorModel(model);

                // update  meta values
                foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> val in Request.Form)
                {
                    if (val.Key.StartsWith("Meta:"))
                    {
                        // bosh we have a meta
                        var metaKey = val.Key.Replace("Meta:", "");
                        if (model.HasMeta(metaKey))
                        {
                            model.UpdateMeta(metaKey, val.Value.ToString());
                        }
                        else
                        {
                            // Add it...
                            CustomField metaDetails = model.Type.GetMetaDetails(val.Key.Replace("Meta:", ""));
                            model.AddMeta(metaDetails.Name, val.Value.ToString(), metaDetails.Type);
                        }
                    }
                }
                string currentTemplate = model.GetMeta("Settings.Template").GetStringValue();
                // delete all template metas that do not exist in the new template, and add any that are missing
                List<string> newMetas = _content.GetMetasForTemplate(currentTemplate, model.Type.TemplateFolder);
                if (newMetas != null)
                    _content.UpdateTemplateMetas(model, newMetas);

                await _content.UpdateAsync(model);

                SaveMessage = "Saved!";
                MessageType = AlertType.Success;
                return View(model);

            }
            catch (Exception ex)
            {
                SaveMessage = "There was a problem saving: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
            }

            return View(model);
        }

        private async Task GenerateNewSlug(Content model)
        {
            KeyGenerator generator = new KeyGenerator();
            model.Slug = generator.UrlSlug();
            while (await _content.SlugExists(model.Slug))
            {
                model.Slug = generator.UrlSlug();
            }
        }
        #endregion

        #region Create
        [Route("admin/content/create/{type}")]
        public IActionResult Create(string type)
        {
            Content model = new Content()
            {
                PublishDate = DateTime.Now,
                Type = Engine.Settings.Content.GetContentType(type),
                Status = ContentStatus.Draft
            };
            return View("_Blade_Content", model);
        }

        [HttpPost]
        [Route("admin/content/create/{type}")]
        public async Task<Response> Create(Content model)
        {
            try
            {
                model.AllowComments = true;
                model.AuthorId = Engine.Account.Id;
                model.Body = "";
                model.CreatedBy = Engine.Account.UserName;
                model.CreatedOn = DateTime.Now;
                model.LastEditedBy = Engine.Account.UserName;
                model.LastEditedOn = DateTime.Now;
                model.Public = true;
                model.ShareCount = 0;
                model.Views = 0;
                model = await _content.AddAsync(model);
                return new Response(true, $"The content was created successfully.<br /><a href='{Url.Action(nameof(Edit), new { id = model.Id })}'>Go to the new content</a>");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error publishing content.", ex);
            }
        }

        #endregion

        #region Content Categories
        [Route("admin/content/{id}/categories/")]
        public async Task<IActionResult> ContentCategories(int id)
        {
            Content content = await _content.GetContentByIdAsync(id);
            EditContentModel model = new EditContentModel()
            {
                ContentType = Engine.Settings.Content.GetContentType(content.ContentType),
                Content = content
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/content/{id}/categories/toggle/")]
        public async Task<Response> ToggleCategory(int id, int categoryId, bool add)
        {
            try
            {
                if (add)
                {
                    await _content.AddCategoryToContentAsync(id, categoryId);
                    _contentCategoryCache.ResetCache();
                    return new Response(true, "Added the category to the content.");
                }
                else
                {
                    await _content.RemoveCategoryFromContentAsync(id, categoryId);
                    _contentCategoryCache.ResetCache();
                    return new Response(true, "Removed the category from the content.");
                }
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error toggling a content category.", ex);
            }
        }

        [Route("admin/content/categories/suggestions/{type}/")]
        public IActionResult CategorySuggestions(string type)
        {
            var suggestions = _contentCategoryCache.GetSuggestions(type).Select(c => new { id = c.Id, displayName = c.DisplayName, slug = c.Slug });
            return Json(suggestions.ToArray());
        }
        #endregion

        #region Manage Categories        
        [Route("admin/content/categories/list/{type}/")]
        public IActionResult Categories(string type)
        {
            ContentModel model = new ContentModel
            {
                ContentType = Engine.Settings.Content.GetContentType(type)
            };
            return View("_List_Categories", model);
        }

        [Route("admin/content/categories/add/{type}/")]
        public IActionResult CreateCategory(string type)
        {
            ContentCategory model = new ContentCategory()
            {
                ContentType = type,
                Categories = _contentCategoryCache.TopLevel(type)
            };
            return View("_Blade_Category", model);
        }

        [HttpPost]
        [Route("admin/content/categories/add/{type}/")]
        public async Task<Response> CreateCategory(ContentCategory category)
        {
            try
            {
                if (!category.DisplayName.IsSet())
                {
                    throw new Exception("You need to enter a category!");
                }

                ContentCategory categoryResult = await _content.AddCategoryAsync(category);
                _contentCategoryCache.ResetCache();

                return new Response(true, "Added the category.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error adding a content category.", ex);
            }
        }

        [Route("admin/content/categories/edit/{id}/")]
        public async Task<IActionResult> EditCategory(int id, string type)
        {
            ContentCategory model = await _content.GetCategoryByIdAsync(id);
            model.Categories = _contentCategoryCache.TopLevel(type);
            return View("_Blade_Category", model);
        }

        [HttpPost]
        [Route("admin/content/categories/edit/{id}/")]
        public async Task<Response> EditCategory(ContentCategory model)
        {
            try
            {
                if (model.ParentCategoryId.HasValue)
                {
                    if (model.ParentCategoryId == model.Id)
                    {
                        throw new Exception("You cannot set the parent to be the same category!");
                    }

                    IEnumerable<ContentCategory> thisAndChildren = _contentCategoryCache.GetThisAndChildren(model.Id);
                    if (thisAndChildren.Select(c => c.Id).ToList().Contains(model.ParentCategoryId.Value))
                    {
                        throw new Exception("You cannot set the parent to be a child of this category!");
                    }
                }

                await _content.UpdateCategoryAsync(model);
                return new Response(true, $"The category has been saved.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error updating a content category.", ex);
            }
        }

        [HttpPost]
        [Route("admin/content/categories/delete/{id}/")]
        public async Task<Response> DeleteCategory(int id)
        {
            try
            {
                await _content.DeleteCategoryAsync(id);
                return new Response(true, $"The category has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error deleting a content category, did you make sure it was empty first?", ex);
            }
        }
        #endregion

        #region Set Status
        [Route("admin/content/set-status/{id}")]
        [HttpPost()]
        public async Task<Response> SetStatus(int id, ContentStatus status)
        {
            try
            {
                await _content.SetStatusAsync(id, status);
                Content content = await _content.GetContentByIdAsync(id);
                return new Response(true, "Content status has been updated successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error publishing content with Id: {id}", ex);
            }
        }
        #endregion

        #region Delete
        [HttpPost()]
        [Route("admin/content/delete/{id}")]
        public async Task<Response> Delete(int id)
        {
            try
            {
                await _content.DeleteAsync(id);
                return new Response(true, "The content has been successfully removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error deleting content with Id: {id}", ex);
            }
        }
        [Authorize(Roles = "SuperUser,Admin")]
        [Route("admin/content/{type}/delete/all/")]
        public async Task<IActionResult> DeleteAll(string type)
        {
            try
            {
                await _content.DeleteAllAsync(type);
                SaveMessage = $"All the content has been deleted.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error deleting all content.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Duplicate
        [Route("admin/content/duplicate/{id}")]
        public async Task<IActionResult> Duplicate(int id)
        {
            try
            {
                Content newContent = await _content.DuplicateContentAsync(id);
                return RedirectToAction(nameof(Edit), new { newContent.Id });
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error duplicating content with Id: {id}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                SaveMessage += $": {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }
        #endregion

        [Route("admin/pages/set-home/{id}")]
        public async Task<Response> SetHomepage(int id)
        {
            try
            {
                _cache.Remove(typeof(BasicSettings).ToString());
                BasicSettings model = Engine.Settings.Basic;
                model.Homepage = id;
                Engine.Settings.Set(model);
                return new Response(true, "The homepage has now been set.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error setting a homepage as content Id: {id}", ex);
            }
        }

        #region Media
        /// <summary>
        /// Attach media file to entity. This is the action which handles the chosen attachment from the media attach action.
        /// </summary>
        [HttpPost]
        [Route("admin/content/{id}/media/upload")]
        public async Task<Response> UploadMedia(int id, AttachMediaModel model)
        {
            try
            {
                model.ValidateOrThrow();

                // load the media object.
                Content content = await _db.Content.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (content == null)
                {
                    throw new Exception("Could not load content to attach media.");
                }

                MediaObject media = _db.Media.SingleOrDefault(m => m.Id == model.MediaId);
                if (media == null)
                {
                    throw new Exception("Could not load media to attach.");
                }

                switch (model.FieldName)
                {
                    case "FeaturedImage":
                        content.FeaturedImage = new ContentMedia(media);
                        break;
                    case "ShareImage":
                        content.ShareImage = new ContentMedia(media);
                        break;
                }

                await _db.SaveChangesAsync();

                string cacheKey = typeof(Content).ToString() + ".Single." + id;
                _cache.Remove(cacheKey);

                return new Response(true, media, $"The media has been attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error attaching a media file to an entity.", ex);
            }
        }

        /// <summary>
        /// Remove media file from entity.
        /// </summary>
        [HttpPost]
        [Route("admin/content/{id}/media/remove")]
        public async Task<Response> RemoveMedia(int id, AttachMediaModel model)
        {
            try
            {
                // load the media object.
                Content content = await _db.Content.Where(p => p.Id == id).FirstOrDefaultAsync();
                MediaObject media = _db.Media.SingleOrDefault(m => m.Id == model.MediaId);
                string cacheKey = typeof(Content).ToString() + ".Single." + id;

                switch (model.FieldName)
                {
                    case nameof(Models.Content.FeaturedImage):
                        content.FeaturedImageJson = null;
                        break;
                    case nameof(Models.Content.ShareImage):
                        content.ShareImageJson = null;
                        break;
                }

                await _db.SaveChangesAsync();
                _cache.Remove(cacheKey);
                return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error removing a media file from an entity.", ex);
            }
        }
        #endregion

        #region Gallery
        [Route("admin/content/{id}/gallery")]
        public async Task<IActionResult> Gallery(int id)
        {
            Content model = await _content.GetContentByIdAsync(id, true);
            if (model == null)
            {
                return NotFound();
            }

            model = await GetEditorModel(model);
            return View("_List_ContentMedia", model);
        }

        [Route("admin/content/media/{id}/upload/gallery")]
        public async Task<Response> UploadToGallery(List<IFormFile> files, int id)
        {

            try
            {
                Content content = await _content.GetContentByIdAsync(id);
                if (content == null)
                {
                    throw new Exception("Content not found!");
                }

                MediaObject mediaResult = null;
                if (files != null)
                {
                    if (files.Count == 0)
                    {
                        throw new Exception("There are no files attached!");
                    }

                    var directory = await _content.GetDirectoryAsync();
                    foreach (IFormFile file in files)
                    {
                        mediaResult = await _media.ProcessUpload(file, _directoryManager.GetPath(directory.Id)) as MediaObject;
                        await _content.AddImageAsync(content, new ContentMedia(mediaResult));
                        mediaResult.DirectoryId = directory.Id;
                        _db.Media.Add(mediaResult);
                        await _db.SaveChangesAsync();
                    }
                }
                return new Response(true, mediaResult, "The image/media file has been attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error uploading media to the gallery.", ex);
            }
        }

        [HttpPost]
        [Route("admin/content/media/{id}/remove/{mediaId}")]
        public async Task<Response> RemoveMedia(int id, int mediaId)
        {
            try
            {
                Content content = await _content.GetContentByIdAsync(id, true);
                ContentMedia media = content.Media.Find(m => m.Id == mediaId);
                if (media != null)
                {
                    var mediaItem = await _db.Media.SingleOrDefaultAsync(m => m.UniqueId == media.UniqueId);
                    if (mediaItem != null)
                    {
                        _db.Entry(mediaItem).State = EntityState.Deleted;
                    }
                    await _media.DeleteStoredMedia(media);
                    _db.Entry(media).State = EntityState.Deleted;
                }

                await _content.UpdateAsync(content);
                return new Response(true, "The image has now been removed.");
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<ContentController>("Error removing media item.", ex);
                return new Response(ex);
            }
        }

        [HttpGet]
        [Route("admin/content/media/{id}/setfeatured/{mediaId}")]
        public async Task<Response> SetFeatured(int id, int mediaId)
        {
            try
            {
                Content content = await _content.GetContentByIdAsync(id, true);
                ContentMedia media = content.Media.SingleOrDefault(m => m.Id == mediaId);
                if (media != null)
                {
                    content.FeaturedImage = new MediaObject(media);
                    await _content.UpdateAsync(content);
                }
                return new Response(true, "The image has now been set as the featured image. You may need to reload the page to see the change.");
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<ContentController>("Error setting a featured image from the media list.", ex);
                return new Response(ex);
            }
        }
        #endregion

        #region Helpers

        protected async Task<Content> GetEditorModel(Content model)
        {

            model.Type = Engine.Settings.Content.GetContentType(model.ContentType);

            UserListModel authors = await _account.GetUserProfilesAsync(new UserListModel() { PageSize = 50 });
            model.Authors = authors.List;

            if (model.Type != null)
            {
                // Templates
                if (model.Type.Templates)
                {
                    model = GetTemplates(model, model.Type.TemplateFolder);
                }
            }

            return model;
        }

        protected Content GetTemplates(Content model, string templateDirectory)
        {
            Dictionary<string, string> templates = new Dictionary<string, string>();

            // Add the base templates:
            string[] files = UserInterfaceProvider.GetFiles("~/UI/" + templateDirectory + "/");
            foreach (string temp in files)
            {
                if (temp.EndsWith(".cshtml"))
                {
                    string key = Path.GetFileNameWithoutExtension(temp);
                    string value = key.TrimStart('_').Replace("_", " ").ToTitleCase();
                    if (!templates.ContainsKey(key))
                    {
                        templates.Add(key, value);
                    }
                }
            }

            string[] templateDirs = {
                _env.ContentRootPath + "\\UI\\" + templateDirectory + "\\",
                _env.ContentRootPath + "\\Views\\" + templateDirectory + "\\",
                _env.ContentRootPath + "\\Themes\\" + Engine.Settings["Hood.Settings.Theme"] + "\\Views\\" + templateDirectory + "\\"
            };

            foreach (string str in templateDirs)
            {
                try
                {
                    files = Directory.GetFiles(str);
                    foreach (string temp in files)
                    {
                        if (temp.EndsWith(".cshtml"))
                        {
                            string key = Path.GetFileNameWithoutExtension(temp);
                            string value = key.TrimStart('_').Replace("_", " ").ToTitleCase();
                            if (!templates.ContainsKey(key))
                            {
                                templates.Add(key, value);
                            }
                        }
                    }
                }
                catch { }
            }

            model.Templates = templates.Distinct().OrderBy(s => s.Key).ToDictionary(t => t.Key, t => t.Value);

            return model;
        }

        #endregion
    }
}


