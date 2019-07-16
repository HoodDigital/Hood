using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.IO;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var content = await _content.GetContentByIdAsync(id, true);
            if (content == null)
                return NotFound();
            Content model = await GetEditorModel(content);
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
                        throw new Exception("The slug is not valid, it already exists or is a reserved system word.");
                }
                else
                {
                    var generator = new KeyGenerator();
                    model.Slug = generator.UrlSlug();
                    while (await _content.SlugExists(model.Slug))
                        model.Slug = generator.UrlSlug();
                }

                var _contentSettings = Engine.Settings.Content;
                var type = _contentSettings.GetContentType(model.ContentType);
                // update  meta values
                var oldTemplate = model.GetMeta("Settings.Template").GetStringValue();
                foreach (var val in Request.Form)
                {
                    if (val.Key.StartsWith("Meta:"))
                    {
                        // bosh we have a meta
                        if (model.HasMeta(val.Key.Replace("Meta:", "")))
                        {
                            model.UpdateMeta(val.Key.Replace("Meta:", ""), val.Value.ToString());
                        }
                        else
                        {
                            // Add it...
                            var metaDetails = type.GetMetaDetails(val.Key.Replace("Meta:", ""));
                            model.AddMeta(new ContentMeta()
                            {
                                ContentId = model.Id,
                                Name = metaDetails.Name,
                                Type = metaDetails.Type,
                                BaseValue = JsonConvert.SerializeObject(val.Value)
                            });

                        }
                    }
                }
                var currentTemplate = model.GetMeta("Settings.Template").GetStringValue();
                // check if new template has been selected
                if (oldTemplate.IsSet() && currentTemplate.IsSet())
                {
                    // delete all template metas that do not exist in the new template, and add any that are missing
                    List<string> newMetas = _content.GetMetasForTemplate(currentTemplate, type.TemplateFolder);
                    _content.UpdateTemplateMetas(model, newMetas);
                }


                await _content.UpdateAsync(model);

                model = await GetEditorModel(model);
                SaveMessage = "Saved!";
                MessageType = AlertType.Success;
                return View(model);

            }
            catch (Exception ex)
            {
                model = await GetEditorModel(model);
                SaveMessage = "There was a problem saving: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return View(model);
            }
        }

        [Route("admin/content/{id}/gallery/")]
        public async Task<IActionResult> EditorGallery(int id)
        {
            var model = await _content.GetContentByIdAsync(id);
            return View(model);
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
            var content = await _content.GetContentByIdAsync(id);
            EditContentModel model = new EditContentModel()
            {
                ContentType = Engine.Settings.Content.GetContentType(content.ContentType),
                Content = content
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/content/{id}/categories/add/")]
        public async Task<Response> AddCategoryToContent(int id, int categoryId)
        {
            try
            {
                await _content.AddCategoryToContentAsync(id, categoryId);
                _contentCategoryCache.ResetCache();
#warning TODO: Handle response in JS.
                return new Response(true, "Added the category to the content.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error adding a content category.", ex);
            }
        }

        [Route("admin/content/{id}/categories/remove/")]
        public async Task<Response> RemoveCategoryAsync(int id, int categoryId)
        {
            try
            {
                await _content.RemoveCategoryFromContentAsync(id, categoryId);
                _contentCategoryCache.ResetCache();
#warning TODO: Handle response in JS.
                return new Response(true, "Removed the category from the content.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error removing a content category.", ex);
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
            var model = new ContentCategory()
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
                    throw new Exception("You need to enter a category!");

                var categoryResult = await _content.AddCategoryAsync(category);
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
            var model = await _content.GetCategoryByIdAsync(id);
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
                        throw new Exception("You cannot set the parent to be the same category!");

                    var thisAndChildren = _contentCategoryCache.GetThisAndChildren(model.Id);
                    if (thisAndChildren.Select(c => c.Id).ToList().Contains(model.ParentCategoryId.Value))
                        throw new Exception("You cannot set the parent to be a child of this category!");
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
                var content = await _content.GetContentByIdAsync(id);
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
                var newContent = await _content.DuplicateContentAsync(id);
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

        [Route("admin/content/sethomepage/{id}")]
        public async Task<Response> SetHomepage(int id)
        {
            try
            {
                _cache.Remove(typeof(BasicSettings).ToString());
                var model = Engine.Settings.Basic;
                model.Homepage = id;
                Engine.Settings.Set(model);
                return new Response(true, "The homepage has now been set.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error setting a homepage as content Id: {id}", ex);
            }
        }

        #region Gallery
        [Route("admin/content/media/{id}/upload/gallery")]
        public async Task<IActionResult> UploadToGallery(List<IFormFile> files, int id)
        {
            // User must have an organisation.
            Content content = await _content.GetContentByIdAsync(id);
            if (content == null)
                return NotFound();

            try
            {
                MediaObject mediaResult = null;
                if (files != null)
                {
                    if (files.Count == 0)
                        return Json(new
                        {
                            Success = false,
                            Error = "There are no files attached!"
                        });

                    foreach (IFormFile file in files)
                    {
                        mediaResult = await _media.ProcessUpload(file, content.DirectoryPath) as MediaObject;
                        await _content.AddImageAsync(content, new ContentMedia(mediaResult));
                    }
                }
                return Json(new { Success = true, Image = mediaResult });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
            }
        }

        [HttpGet]
        [Route("admin/content/media/{id}/remove/{mediaId}")]
        public async Task<IActionResult> RemoveMedia(int id, int mediaId)
        {
            try
            {
                Content content = await _content.GetContentByIdAsync(id, true);
                ContentMedia media = content.Media.Find(m => m.Id == mediaId);
                if (media != null)
                    content.Media.Remove(media);
                await _content.UpdateAsync(content);
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<ContentController>("Error removing media item", ex);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpGet]
        [Route("admin/content/media/{id}/setfeatured/{mediaId}")]
        public async Task<IActionResult> SetFeaturedAsync(int id, int mediaId)
        {
            Content content = await _content.GetContentByIdAsync(id, true);
            ContentMedia media = content.Media.SingleOrDefault(m => m.Id == mediaId);
            if (media != null)
            {
                content.FeaturedImage = new MediaObject(media);
                await _content.UpdateAsync(content);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }
        #endregion

        #region Helpers

        protected async Task<Content> GetEditorModel(Content model)
        {
            model.Type = Engine.Settings.Content.GetContentType(model.ContentType);

#warning Replace this with a client side lookup.
            var authors = await _account.GetUserProfilesAsync(new UserListModel() { PageSize = 50 });
            model.Authors = authors.List;

            if (model.Type != null)
            {
                // Templates
                if (model.Type.Templates)
                    model = GetTemplates(model, model.Type.TemplateFolder);

                // Special type features.
                switch (model.Type.BaseName)
                {
                    case "Page":
                        if (Engine.Settings.Billing.CheckSubscriptionsOrThrow())
                        {
                            // get subscriptions - if there are any.
                            var subs = await _account.GetSubscriptionPlansAsync(new SubscriptionSearchModel() { PageSize = int.MaxValue });
                            model.Subscriptions = subs.List;
                        }
                        break;
                }
            }

            return model;
        }

        protected Content GetTemplates(Content model, string templateDirectory)
        {
            Dictionary<string, string> templates = new Dictionary<string, string>();

            // Add the base templates:
            var files = EmbeddedFiles.GetFiles("~/UI/" + templateDirectory + "/");
            foreach (var temp in files)
            {
                if (temp.EndsWith(".cshtml"))
                {
                    var key = Path.GetFileNameWithoutExtension(temp);
                    var value = key.TrimStart('_').Replace("_", " ").ToTitleCase();
                    if (!templates.ContainsKey(key))
                        templates.Add(key, value);
                }
            }

            string[] templateDirs = {
                _env.ContentRootPath + "\\Views\\" + templateDirectory + "\\",
                _env.ContentRootPath + "\\Themes\\" + Engine.Settings["Hood.Settings.Theme"] + "\\Views\\" + templateDirectory + "\\"
            };

            foreach (string str in templateDirs)
            {
                try
                {
                    files = Directory.GetFiles(str);
                    foreach (var temp in files)
                    {
                        if (temp.EndsWith(".cshtml"))
                        {
                            var key = Path.GetFileNameWithoutExtension(temp);
                            var value = key.TrimStart('_').Replace("_", " ").ToTitleCase();
                            if (!templates.ContainsKey(key))
                                templates.Add(key, value);
                        }
                    }
                }
                catch { }
            }

            model.Templates = templates.Distinct().OrderBy(s => s.Key).ToDictionary(t => t.Key, t => t.Value);

            return model;
        }

        #endregion

        #region Designer (Beta)
        [HttpPost()]
        [Route("admin/content/designer/save/")]
        public async Task<Response> SaveDesigner(DesignContentModel post)
        {
            try
            {
                var content = await _content.GetContentByIdAsync(post.Id, true);
                content.Body = post.Body;
                content.Body = Regex.Replace(content.Body, @"contenteditable=""true""", "", RegexOptions.IgnoreCase);
                content.Body = Regex.Replace(content.Body, @"id=""mce_[^;]+""", "", RegexOptions.IgnoreCase);
                content.Body = Regex.Replace(content.Body, @"mce-content-body", "", RegexOptions.IgnoreCase);
                await _content.UpdateAsync(content);
                return new Response(true, $"The designer view has been saved.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error saving the designer view.", ex);
            }
        }
        [Route("admin/content/choose/")]
        public IActionResult Choose()
        {
            return View();
        }
        [Route("admin/content/blocks/")]
        public IActionResult Blocks(ListFilters request)
        {
            string[] templateDirs = {
                    _env.ContentRootPath + "\\Themes\\" + Engine.Settings["Hood.Settings.Theme"] + "\\Views\\Blocks\\",
                    _env.ContentRootPath + "\\Views\\Blocks\\"
                };
            List<BlockContent> templates = new List<BlockContent>();
            foreach (string str in templateDirs)
            {
                if (Directory.Exists(str))
                    foreach (string temp in Directory.GetFiles(str).Where(s => s.Contains(".cshtml")))
                    {
                        string name = temp.Replace(str, "").Replace(".cshtml", "").ToSentenceCase().ToTitleCase();
                        try
                        {
                            var jsonFile = temp.Replace(".cshtml", ".json");
                            string json = System.IO.File.ReadAllText(jsonFile);
                            ContentBlockSettings settings = JsonConvert.DeserializeObject<ContentBlockSettings>(json);
                            if (!templates.Select(t => t.Name).Contains(name))
                                templates.Add(new BlockContent()
                                {
                                    Name = settings.Name,
                                    Description = settings.Description,
                                    Variables = settings.Variables,
                                    Url = temp.Replace(str, "")
                                });
                        }
                        catch
                        {
                            if (!templates.Select(t => t.Name).Contains(name))
                                templates.Add(new BlockContent()
                                {
                                    Name = name,
                                    Description = "No description added...",
                                    Url = temp.Replace(str, "")
                                });
                        }
                    }
            }

            // Add the base templates:
            foreach (string temp in EmbeddedFiles.GetFiles("~/Views/Blocks/").Where(s => s.Contains(".cshtml")))
            {
                string name = temp.Replace(".cshtml", "").ToSentenceCase().ToTitleCase();
                try
                {
                    var jsonFile = temp.Replace(".cshtml", ".json");
                    string json = EmbeddedFiles.ReadAllText("~/Views/Blocks/" + jsonFile);
                    ContentBlockSettings settings = JsonConvert.DeserializeObject<ContentBlockSettings>(json);
                    if (!templates.Select(t => t.Name).Contains(name))
                        templates.Add(new BlockContent()
                        {
                            Name = settings.Name,
                            Description = settings.Description,
                            Variables = settings.Variables,
                            Url = temp
                        });
                }
                catch
                {
                    if (!templates.Select(t => t.Name).Contains(name))
                        templates.Add(new BlockContent()
                        {
                            Name = name,
                            Description = "No description added...",
                            Url = temp
                        });
                }
            }

            Response response = new Response(templates.OrderBy(t => t.Name).Skip(request.skip).Take(request.take).ToArray(), templates.Count());
            return Json(response);
        }
        [Route("admin/content/block/")]
        public IActionResult Block(string url)
        {
            string[] templateDirs = {
                _env.ContentRootPath + "\\Themes\\" + Engine.Settings["Hood.Settings.Theme"] + "\\Views\\Blocks\\",
                _env.ContentRootPath + "\\Views\\Blocks\\"
            };
            string[] templateVirtualDirs = {
                "~/Themes/" + Engine.Settings["Hood.Settings.Theme"] + "/Views/Blocks/",
                "~/Views/Blocks/"
            };
            for (int i = 0; i < templateDirs.Length; i++)
            {
                try
                {
                    string view = templateDirs[i] + url;
                    if (System.IO.File.Exists(view))
                        return View(templateVirtualDirs[i] + url);
                    view = templateVirtualDirs[i] + url;
                    if (EmbeddedFiles.GetFiles(view).Count() == 1)
                        return View(templateVirtualDirs[i] + url);
                }
                catch { }
            }
            return NotFound();
        }
        #endregion

    }
}


