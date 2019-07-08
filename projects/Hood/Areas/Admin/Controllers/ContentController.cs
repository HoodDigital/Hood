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
    [Authorize(Roles = "Admin,Editor,Manager")]
    public class ContentController : BaseController
    {
        public ContentController()
            : base()
        {
        }

        [Route("admin/content/{type}/manage/")]
        public async Task<IActionResult> Index(ContentModel model)
        {
            model = await _content.GetContentAsync(model);
            model.ContentType = Engine.Settings.Content.GetContentType(model.Type);
            return View(model);
        }

        [Route("admin/content/{id}/edit/")]
        public async Task<IActionResult> Edit(int id)
        {
            var content = await _content.GetContentByIdAsync(id, true);
            if (content == null)
                return NotFound();
            EditContentModel model = await GetEditorModel(content);
            return View(model);
        }

        [HttpPost()]
        [Route("admin/content/{id}/edit/")]
        public async Task<ActionResult> Edit(EditContentModelSend post)
        {
            var content = await _content.GetContentByIdAsync(post.Id, true);
            try
            {
                post.PublishDate = post.PublishDate.AddHours(post.PublishHour);
                post.PublishDate = post.PublishDate.AddMinutes(post.PublishMinute);
                post.CopyProperties(content);
                content.AuthorId = post.AuthorId;
                content.Featured = post.Featured;
                content.LastEditedBy = User.Identity.Name;
                content.LastEditedOn = DateTime.Now;

                if (content.Slug.IsSet())
                {
                    if (!await _content.CheckSlugAsync(content.Slug, content.Id))
                        throw new Exception("The slug is not valid, it already exists or is a reserved system word.");
                }
                else
                {
                    var generator = new KeyGenerator();
                    content.Slug = generator.UrlSlug();
                    while (!await _content.CheckSlugAsync(content.Slug))
                        content.Slug = generator.UrlSlug();
                }

                List<string> tags = post.Tags?.Split(',').ToList();
                if (tags == null)
                    tags = new List<string>();
                foreach (string tag in tags)
                {
                    // check if it exists in the db, if not add it. 
                    var tagResult = await _content.AddTagAsync(tag);

                    // add it to the model object.
                    content.AddTag(tagResult.Value);
                }
                if (content.Tags != null)
                {
                    var extraneous = content.Tags.Where(p => !tags.Select(t => t.Trim().ToTitleCase()).Any(p2 => p2 == p.TagId)).Select(s => s.TagId).ToArray();
                    for (int i = 0; i < extraneous.Count(); i++)
                        content.RemoveTag(extraneous[i]);
                }
                var _contentSettings = Engine.Settings.Content;
                var type = _contentSettings.GetContentType(content.ContentType);
                // update  meta values
                var oldTemplate = content.GetMeta("Settings.Template").GetStringValue();
                foreach (var val in Request.Form)
                {
                    if (val.Key.StartsWith("Meta:"))
                    {
                        // bosh we have a meta
                        if (content.HasMeta(val.Key.Replace("Meta:", "")))
                        {
                            content.UpdateMeta(val.Key.Replace("Meta:", ""), val.Value.ToString());
                        }
                        else
                        {
                            // Add it...
                            var metaDetails = type.GetMetaDetails(val.Key.Replace("Meta:", ""));
                            content.AddMeta(new ContentMeta()
                            {
                                ContentId = content.Id,
                                Name = metaDetails.Name,
                                Type = metaDetails.Type,
                                BaseValue = JsonConvert.SerializeObject(val.Value)
                            });

                        }
                    }
                }
                var currentTemplate = content.GetMeta("Settings.Template").GetStringValue();
                // check if new template has been selected
                if (oldTemplate.IsSet() && currentTemplate.IsSet())
                {
                    // delete all template metas that do not exist in the new template, and add any that are missing
                    List<string> newMetas = _content.GetMetasForTemplate(currentTemplate, type.TemplateFolder);
                    _content.UpdateTemplateMetas(content, newMetas);
                }


                await _content.UpdateAsync(content);

                EditContentModel model = await GetEditorModel(content);
                SaveMessage = "Saved!";
                MessageType = AlertType.Success;
                return View(model);

            }
            catch (Exception ex)
            {
                EditContentModel model = await GetEditorModel(content);
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
                return new Response(true);
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while saving the designer view: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/content/{type}/create/")]
        public IActionResult Create(string type)
        {
            Content model = new Content()
            {
                PublishDate = DateTime.Now,
                Type = Engine.Settings.Content.GetContentType(type)
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/content/create/")]
        public async Task<Response> Create(Content model)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);
                model.AllowComments = true;
                model.AuthorId = user.Id;
                model.Body = "";
                model.CreatedBy = user.UserName;
                model.CreatedOn = DateTime.Now;
                model.LastEditedBy = user.UserName;
                model.LastEditedOn = DateTime.Now;
                model.Public = true;
                model.ShareCount = 0;
                model.Views = 0;
                await _content.AddAsync(model);
#warning TODO: Handle response in JS.
                return new Response(true, "Published successfully.");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while publishing: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/content/{id}/categories/")]
        public async Task<IActionResult> CategoriesAsync(int id)
        {
            var content = await _content.GetContentByIdAsync(id);
            EditContentModel model = new EditContentModel()
            {
                ContentType = Engine.Settings.Content.GetContentType(content.ContentType),
                Content = content
            };
            return View(model);
        }

        [Route("admin/content/categories/{type}/")]
        public IActionResult CategoryList(string type)
        {
            ContentModel model = new ContentModel
            {
                ContentType = Engine.Settings.Content.GetContentType(type)
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/content/categories/add/")]
        public async Task<Response> AddCategoryToContent(int contentId, int categoryId)
        {
            try
            {
                await _content.AddCategoryToContentAsync(contentId, categoryId);
                _contentCategoryCache.ResetCache();
#warning TODO: Handle response in JS.
                return new Response(true, "Added the category to the content.");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while adding a content category: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/content/categories/remove/")]
        public async Task<Response> RemoveCategoryAsync(int contentId, int categoryId)
        {
            try
            {
                await _content.RemoveCategoryFromContentAsync(contentId, categoryId);
                _contentCategoryCache.ResetCache();
#warning TODO: Handle response in JS.
                return new Response(true, "Removed the category from the content.");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while removing a content category: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [HttpPost]
        [Route("admin/categories/add/")]
        public async Task<Response> AddCategory(ContentCategory category)
        {
            try
            {
                if (!category.DisplayName.IsSet())
                    throw new Exception("You need to enter a category!");

                var categoryResult = await _content.AddCategoryAsync(category);
                _contentCategoryCache.ResetCache();

                #warning TODO: Handle response in JS.
                return new Response(true, "Added the category.");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while adding a content category: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/categories/edit/{id}/")]
        public async Task<IActionResult> EditCategory(int id, string type)
        {
            var model = await _content.GetCategoryByIdAsync(id);
            model.Categories = _contentCategoryCache.TopLevel(type);
            return View(model);
        }

        [Route("admin/categories/save/")]
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
                return new Response(true);
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while updating a category: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/categories/delete/{id}/")]
        public async Task<Response> DeleteCategory(int id)
        {
            try
            {
                await _content.DeleteCategoryAsync(id);
                return new Response(true);
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while deleting a category, did you make sure it was empty first?";
                await _logService.AddExceptionAsync<ContentController>($"Error deleting a content category with Id: {id}", ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/content/choose/")]
        public IActionResult Choose()
        {
            return View();
        }
        [Route("admin/content/blocks/")]
        public IActionResult Blocks(ListFilters request, string search, string sort, string type)
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

        [Route("admin/content/{id}/publish")]
        [HttpPost()]
        public async Task<Response> Publish(int id)
        {
            try
            {
                await _content.SetStatusAsync(id, ContentStatus.Published);
                var content = await _content.GetContentByIdAsync(id);
                return new Response(true, "Published successfully.");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while publishing content with Id {id}: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }
        [Route("admin/content/{id}/archive")]
        [HttpPost()]
        public async Task<Response> Archive(int id)
        {
            try
            {
                await _content.SetStatusAsync(id, ContentStatus.Archived);
                var content = await _content.GetContentByIdAsync(id);
                return new Response(true, "Archived successfully.");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while archiving content with Id {id}: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [HttpPost()]
        [Route("admin/content/{id}/delete")]
        public async Task<Response> Delete(int id)
        {
            try
            {
                await _content.DeleteAsync(id);
                return new Response(true, "Deleted!");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while deleting content with Id {id}: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/content/{id}/duplicate")]
        public async Task<IActionResult> Duplicate(int id)
        {
            try
            {
                var newContent = await _content.DuplicateContentAsync(id);
                return RedirectToAction(nameof(Edit), new { newContent.Id });
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while duplicating: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        [Route("admin/content/{id}/sethomepage")]
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
                SaveMessage = $"An error occurred while setting a homepage: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/content/{id}/upload/gallery")]
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
                        mediaResult = await _media.ProcessUpload(file, new MediaObject() { Directory = content.ContentType.ToTitleCase() });
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
        [Route("admin/content/{id}/media/remove/{mediaId}")]
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
        [Route("admin/content/{id}/media/setfeatured/{mediaId}")]
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

        [Route("admin/content/getfeaturedimage/{id}")]
        public async Task<IMediaObject> GetFeaturedImageAsync(int id)
        {
            try
            {
                Content content = await _content.GetContentByIdAsync(id);
                if (content != null && content.FeaturedImage != null)
                    return content.FeaturedImage;
                else
                    throw new Exception("No featured image found");
            }
            catch (Exception)
            {
                return ContentMedia.Blank;
            }
        }

        [Route("admin/content/getsharerimage/{id}")]
        public async Task<IMediaObject> GetSharerImageAsync(int id)
        {
            try
            {
                Content content = await _content.GetContentByIdAsync(id);
                if (content != null && content.ShareImage != null)
                    return content.ShareImage;
                else
                    throw new Exception("No sharer image found");
            }
            catch (Exception)
            {
                return ContentMedia.Blank;
            }
        }

        [Route("admin/content/getmetaimage/{id}")]
        public async Task<IMediaObject> GetMetaImageAsync(int id, string field)
        {
            try
            {
                Content content = await _content.GetContentByIdAsync(id);
                if (content != null)
                    return content.GetMeta(field).Get<IMediaObject>();
                else
                    throw new Exception("No featured image found");
            }
            catch (Exception)
            {
                return new ContentMedia(MediaObject.Blank);
            }
        }

        [Route("admin/content/clearimage/{id}")]
        public async Task<Response> ClearImage(int id)
        {
            try
            {
                Content content = await _content.GetContentByIdAsync(id);
                content.FeaturedImage = null;
                await _content.UpdateAsync(content);
#warning TODO: Handle response in JS.
                return new Response(true, "The image has been cleared!");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while clearing a content image: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/content/clearshareimage/{id}")]
        public async Task<Response> ClearShareImage(int id)
        {
            try
            {
                Content content = await _content.GetContentByIdAsync(id);
                content.ShareImage = null;
                await _content.UpdateAsync(content);
#warning TODO: Handle response in JS.
                return new Response(true, "The image has been cleared!");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while clearing a content share image: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

        [Route("admin/content/clearmeta/{id}")]
        public async Task<Response> ClearMeta(int id, string field)
        {
            try
            {
                var content = await _content.GetContentByIdAsync(id);
                content.UpdateMeta(field, "");
                await _content.UpdateAsync(content);
#warning TODO: Handle response in JS.
                return new Response(true, "The data has been cleared!");
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while clearing a content meta object: {ex.Message}";
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
                return new Response(SaveMessage);
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
                SaveMessage = $"An error occurred while : {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<ContentController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }

        [Route("admin/content/categories/suggestions/{type}/")]
        public IActionResult CategorySuggestions(string type)
        {
            var suggestions = _contentCategoryCache.GetSuggestions(type).Select(c => new { id = c.Id, displayName = c.DisplayName, slug = c.Slug });
            return Json(suggestions.ToArray());
        }

        #region "Helpers"

        protected async Task<EditContentModel> GetEditorModel(Content content)
        {
            EditContentModel model = new EditContentModel()
            {
                ContentType = Engine.Settings.Content.GetContentType(content.ContentType),
                Content = content
            };
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var editors = await _userManager.GetUsersInRoleAsync("Editor");
            model.Authors = editors.Concat(admins).Distinct().OrderBy(u => u.FirstName).ThenBy(u => u.Email).ToList();

            // Templates
            if (model.ContentType.Templates)
                model = GetTemplates(model, model.ContentType.TemplateFolder);

            // Special type features.
            switch (model.ContentType.BaseName)
            {
                case "Page":
                    model = await GetPageEditorFeatures(model);
                    break;
            }

            return model;
        }

        protected async Task<EditContentModel> GetPageEditorFeatures(EditContentModel model)
        {

            if (Engine.Settings.Billing.CheckSubscriptionsOrThrow())
            {
                // get subscriptions - if there are any.
                var subs = await _account.GetSubscriptionPlansAsync(new SubscriptionSearchModel() { PageSize = int.MaxValue });
                model.Subscriptions = subs.List;
            }
            return model;
        }

        protected EditContentModel GetTemplates(EditContentModel model, string templateDirectory)
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

    }
}


