using Hood.Caching;
using Hood.Contexts;
using Hood.BaseControllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Admin.BaseControllers
{
    public abstract class BaseContentController : BaseController
    {
        protected readonly ContentContext _contentDb;
        protected readonly IContentRepository _content;
        protected readonly ContentCategoryCache _contentCategoryCache;
        protected readonly IHoodAccountRepository _account;
        public BaseContentController()
            : base()
        {
            _contentDb = Engine.Services.Resolve<ContentContext>();
            _content = Engine.Services.Resolve<IContentRepository>();
            _contentCategoryCache = Engine.Services.Resolve<ContentCategoryCache>();
            _account = Engine.Services.Resolve<IHoodAccountRepository>();
        }

        [Route("admin/content/manage/{type?}/")]
        public virtual async Task<IActionResult> Index(ContentModel model) => await List(model, "Index");

        [Route("admin/content/list/{type?}/")]
        public virtual async Task<IActionResult> List(ContentModel model, string viewName = "_List_Content")
        {
            model = await _content.GetContentAsync(model);
            model.ContentType = Engine.Settings.Content.GetContentType(model.Type);
            return View(viewName, model);
        }

        #region Edit
        [Route("admin/content/{id}/edit/")]
        public virtual async Task<IActionResult> Edit(int id)
        {
            var model = await _content.GetContentByIdAsync(id, true, false);
            model = await GetEditorModel(model);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost()]
        [Route("admin/content/{id}/edit/")]
        public virtual async Task<ActionResult> Edit(Content model)
        {
            try
            {
                var modelToUpdate = await _content.GetContentByIdAsync(model.Id, true, false);
                modelToUpdate = await GetEditorModel(modelToUpdate);

                var updatedFields = Request.Form.Keys.ToHashSet();
                modelToUpdate = modelToUpdate.UpdateFromFormModel(model, updatedFields);

                modelToUpdate.LastEditedBy = User.Identity.Name;
                modelToUpdate.LastEditedOn = DateTime.UtcNow;

                if (modelToUpdate.Slug.IsSet())
                {
                    if (await _content.SlugExists(modelToUpdate.Slug, modelToUpdate.Id))
                    {
                        modelToUpdate.Type = Engine.Settings.Content.GetContentType(modelToUpdate.ContentType);
                        if (modelToUpdate.Type.BaseName == "Page")
                        {
                            throw new Exception("The slug is not valid, it already exists or is a reserved system word.");
                        }
                        else
                        {
                            await GenerateNewSlug(modelToUpdate);
                        }
                    }
                }
                else
                {
                    await GenerateNewSlug(modelToUpdate);
                }


                // update  meta values
                foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> val in Request.Form)
                {
                    if (val.Key.StartsWith("Meta:"))
                    {
                        // bosh we have a meta
                        var metaKey = val.Key.Replace("Meta:", "");
                        if (modelToUpdate.HasMeta(metaKey))
                        {
                            modelToUpdate.UpdateMeta(metaKey, val.Value.ToString());
                        }
                        else
                        {
                            // Add it...
                            CustomField metaDetails = modelToUpdate.Type.GetMetaDetails(val.Key.Replace("Meta:", ""));
                            modelToUpdate.AddMeta(metaDetails.Name, val.Value.ToString(), metaDetails.Type);
                        }
                    }
                }

                string currentTemplate = modelToUpdate.GetMeta("Settings.Template").GetStringValue();

                // delete all template metas that do not exist in the new template, and add any that are missing
                List<string> newMetas = _content.GetMetasForTemplate(currentTemplate, modelToUpdate.Type.TemplateFolder);
                if (newMetas != null)
                    _content.UpdateTemplateMetas(modelToUpdate, newMetas);

                await _content.UpdateAsync(modelToUpdate);

                SaveMessage = "Saved!";
                MessageType = AlertType.Success;
                return View(modelToUpdate);

            }
            catch (Exception ex)
            {
                SaveMessage = "There was a problem saving: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<BaseContentController>(SaveMessage, ex);
                model.Metadata = await _contentDb.ContentMetadata.Where(cm => cm.ContentId == model.Id).ToListAsync();
                model = await GetEditorModel(model);
                return View(model);
            }

        }

        protected virtual async Task GenerateNewSlug(Content model)
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
        public virtual IActionResult Create(string type)
        {
            Content model = new()
            {
                PublishDate = DateTime.UtcNow,
                Type = Engine.Settings.Content.GetContentType(type),
                Status = ContentStatus.Draft
            };
            return View("_Blade_Content", model);
        }

        [HttpPost]
        [Route("admin/content/create/{type}")]
        public virtual async Task<Response> Create(Content model)
        {
            try
            {
                model.AllowComments = true;
                model.AuthorId = User.GetLocalUserId();
                model.Body = "";
                model.CreatedBy = User.Identity.Name;
                model.CreatedOn = DateTime.UtcNow;
                model.LastEditedBy = User.Identity.Name;
                model.LastEditedOn = DateTime.UtcNow;
                model.Public = true;
                model.ShareCount = 0;
                model.Views = 0;
                model = await _content.AddAsync(model);
                return new Response(true, $"The content was created successfully.<br /><a href='{Url.Action(nameof(Edit), new { id = model.Id })}'>Go to the new content</a>");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseContentController>($"Error publishing content.", ex);
            }
        }

        #endregion

        #region Content Categories
        [Route("admin/content/{id}/categories/")]
        public virtual async Task<IActionResult> ContentCategories(int id)
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
        public virtual async Task<Response> ToggleCategory(int id, int categoryId, bool add)
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
                return await ErrorResponseAsync<BaseContentController>($"Error toggling a content category.", ex);
            }
        }

        [Route("admin/content/categories/suggestions/{type}/")]
        public virtual IActionResult CategorySuggestions(string type)
        {
            var suggestions = _contentCategoryCache.GetSuggestions(type).Select(c => new { id = c.Id, displayName = c.DisplayName, slug = c.Slug });
            return Json(suggestions.ToArray());
        }
        #endregion

        #region Manage Categories        
        [Route("admin/content/categories/list/{type}/")]
        public virtual IActionResult Categories(string type)
        {
            ContentModel model = new ContentModel
            {
                ContentType = Engine.Settings.Content.GetContentType(type)
            };
            return View("_List_Categories", model);
        }

        [Route("admin/content/categories/list-content/{id}/")]
        public virtual async Task<IActionResult> CategoriesContentAsync(int id)
        {
            var model = await _content.GetContentByIdAsync(id, true, false);
            return View("_List_Categories_Content", model);
        }

        [Route("admin/content/categories/add/{type}/")]
        public virtual IActionResult CreateCategory(string type)
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
        public virtual async Task<Response> CreateCategory(ContentCategory category)
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
                return await ErrorResponseAsync<BaseContentController>($"Error adding a content category.", ex);
            }
        }

        [Route("admin/content/categories/edit/{id}/")]
        public virtual async Task<IActionResult> EditCategory(int id, string type)
        {
            ContentCategory model = await _content.GetCategoryByIdAsync(id);
            model.Categories = _contentCategoryCache.TopLevel(type);
            return View("_Blade_Category", model);
        }

        [HttpPost]
        [Route("admin/content/categories/edit/{id}/")]
        public virtual async Task<Response> EditCategory(ContentCategory model)
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
                return await ErrorResponseAsync<BaseContentController>($"Error updating a content category.", ex);
            }
        }

        [HttpPost]
        [Route("admin/content/categories/delete/{id}/")]
        public virtual async Task<Response> DeleteCategory(int id)
        {
            try
            {
                await _content.DeleteCategoryAsync(id);
                return new Response(true, $"The category has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseContentController>($"Error deleting a content category, did you make sure it was empty first?", ex);
            }
        }
        #endregion

        #region Set Status
        [Route("admin/content/set-status/{id}")]
        [HttpPost()]
        public virtual async Task<Response> SetStatus(int id, ContentStatus status)
        {
            try
            {
                await _content.SetStatusAsync(id, status);
                Content content = await _content.GetContentByIdAsync(id);
                return new Response(true, "Content status has been updated successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseContentController>($"Error publishing content with Id: {id}", ex);
            }
        }
        #endregion

        #region Delete
        [HttpPost()]
        [Route("admin/content/delete/{id}")]
        public virtual async Task<Response> Delete(int id)
        {
            try
            {
                await _content.DeleteAsync(id);
                return new Response(true, "The content has been successfully removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseContentController>($"Error deleting content with Id: {id}", ex);
            }
        }

        [Route("admin/content/{type}/delete/all/")]
        public virtual async Task<IActionResult> DeleteAll(string type)
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
                await _logService.AddExceptionAsync<BaseContentController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Duplicate
        [Route("admin/content/duplicate/{id}")]
        public virtual async Task<IActionResult> Duplicate(int id)
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
                await _logService.AddExceptionAsync<BaseContentController>(SaveMessage, ex);
                SaveMessage += $": {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }
        #endregion

        [Route("admin/pages/set-home/{id}")]
        public virtual async Task<Response> SetHomepage(int id)
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
                return await ErrorResponseAsync<BaseContentController>($"Error setting a homepage as content Id: {id}", ex);
            }
        }

        #region Media
        /// <summary>
        /// Attach media file to entity. This is the action which handles the chosen attachment from the media attach action.
        /// </summary>
        [HttpPost]
        [Route("admin/content/{id}/media/upload")]
        public virtual async Task<Response> UploadMedia(int id, AttachMediaModel model)
        {
            try
            {
                model.ValidateOrThrow();

                // load the media object.
                Content content = await _contentDb.Content.Where(p => p.Id == id).FirstOrDefaultAsync();
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
                return await ErrorResponseAsync<BaseContentController>($"Error attaching a media file to an entity.", ex);
            }
        }

        /// <summary>
        /// Remove media file from entity.
        /// </summary>
        [HttpPost]
        [Route("admin/content/{id}/media/remove")]
        public virtual async Task<Response> RemoveMedia(int id, AttachMediaModel model)
        {
            try
            {
                // load the media object.
                Content content = await _contentDb.Content.Where(p => p.Id == id).FirstOrDefaultAsync();
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
                return await ErrorResponseAsync<BaseContentController>($"Error removing a media file from an entity.", ex);
            }
        }
        #endregion

        #region Gallery
        [Route("admin/content/{id}/gallery")]
        public virtual async Task<IActionResult> Gallery(int id)
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
        public virtual async Task<Response> UploadToGallery(List<int> media, int id)
        {

            try
            {
                Content content = await _content.GetContentByIdAsync(id);
                if (content == null)
                {
                    throw new Exception("Content not found!");
                }

                if (media != null)
                {
                    if (media.Count == 0)
                    {
                        throw new Exception("There are no files selected!");
                    }

                    var directory = await _content.GetDirectoryAsync();
                    foreach (int mediaId in media)
                    {
                        // load the media object from db
                        MediaObject mediaObject = _db.Media.AsNoTracking().SingleOrDefault(m => m.Id == mediaId);
                        if (media == null)
                        {
                            throw new Exception("Could not load media to attach.");
                        }
                        var propertyMedia = new ContentMedia(mediaObject);
                        propertyMedia.ContentId = content.Id;
                        propertyMedia.Id = 0;
                        _contentDb.ContentMedia.Add(propertyMedia);
                        await _db.SaveChangesAsync();

                    }
                }
                return new Response(true, "The media has been attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseContentController>($"Error uploading media to the gallery.", ex);
            }
        }

        [HttpPost]
        [Route("admin/content/media/{id}/remove/{mediaId}")]
        public virtual async Task<Response> RemoveMedia(int id, int mediaId)
        {
            try
            {
                ContentMedia media = await _contentDb.ContentMedia.SingleOrDefaultAsync(m => m.Id == mediaId);
                _db.Entry(media).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
                return new Response(true, "The image has now been removed.");
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<BaseContentController>("Error removing media item.", ex);
                return new Response(ex);
            }
        }

        #endregion

        #region Helpers

        protected virtual async Task<Content> GetEditorModel(Content model)
        {

            model.Type = Engine.Settings.Content.GetContentType(model.ContentType);

            model.Authors = await _account.GetUsersInRole("Editor");

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

        protected virtual Content GetTemplates(Content model, string templateDirectory)
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


        [Route("admin/settings/content/")]
        public virtual IActionResult Content()
        {
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings model = Engine.Settings.Content;
            if (model == null)
                model = new ContentSettings();
            return View(model);
        }

        [HttpPost]
        [Route("admin/settings/content/")]
        public virtual async Task<IActionResult> Content(ContentSettings model)
        {
            try
            {
                model.CheckBaseFields();

                Engine.Settings.Set(model);

                // refresh all content metas and things
                await RefreshAllMetasAsync();


                SaveMessage = "Settings saved!";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "Error saving: " + ex.Message;
                MessageType = AlertType.Danger;
            }
            return View(model);
        }

        public virtual async Task RefreshAllMetasAsync()
        {
            foreach (var content in _contentDb.Content.Include(p => p.Metadata).AsNoTracking().ToList())
            {
                var type = Engine.Settings.Content.GetContentType(content.ContentType);
                if (type != null)
                {
                    await _content.RefreshMetasAsync(content);
                    var currentTemplate = content.GetMeta("Settings.Template");
                    if (currentTemplate.GetStringValue().IsSet())
                    {
                        var template = currentTemplate.GetStringValue();
                        if (template.IsSet())
                        {
                            // delete all template metas that do not exist in the new template, and add any that are missing
                            List<string> newMetas = _content.GetMetasForTemplate(template, type.TemplateFolder);
                            if (newMetas != null)
                                _content.UpdateTemplateMetas(content, newMetas);
                        }
                    }
                }
            }
            await _db.SaveChangesAsync();
        }

    }


}


