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
    public class ContentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ContentCategoryCache _categories;
        private readonly IContentRepository _content;
        private readonly ISettingsRepository _settings;
        private readonly IHostingEnvironment _env;
        private readonly IMediaManager<MediaObject> _media;
        private readonly IAccountRepository _auth;

        public ContentController(
            IAccountRepository auth,
            IContentRepository content,
            ContentCategoryCache categories,
            UserManager<ApplicationUser> userManager,
            ISettingsRepository settings,
            IBillingService billing,
            IMediaManager<MediaObject> media,
            IHostingEnvironment env)
        {
            _auth = auth;
            _media = media;
            _userManager = userManager;
            _content = content;
            _settings = settings;
            _categories = categories;
            _env = env;
        }

        [Route("admin/content/{type}/manage/")]
        public async Task<IActionResult> Index(ContentModel model, EditorMessage? message)
        {
            var qry = Request.Query;
            model = await _content.GetPagedContent(model, false);
            model.ContentType = _settings.GetContentSettings().GetContentType(model.Type);
            model.AddEditorMessage(message);
            return View(model);
        }

        [Route("admin/content/{id}/edit/")]
        public async Task<IActionResult> Edit(int id, EditorMessage? message)
        {
            var content = _content.GetContentByID(id, true);
            if (content == null)
                return NotFound();
            EditContentModel model = await GetEditorModel(content);
            model.Content.AddEditorMessage(message);
            return View(model);
        }

        [HttpPost()]
        [Route("admin/content/{id}/edit/")]
        public async Task<ActionResult> Edit(EditContentModelSend post)
        {
            var content = _content.GetContentByID(post.Id, true);
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
                    if (!_content.CheckSlug(content.Slug, content.Id))
                        throw new Exception("The slug is not valid, it already exists or is a reserved system word.");
                }
                else
                {
                    var generator = new KeyGenerator();
                    content.Slug = generator.UrlSlug();
                    while (!_content.CheckSlug(content.Slug))
                        content.Slug = generator.UrlSlug();
                }

                List<string> tags = post.Tags?.Split(',').ToList();
                if (tags == null)
                    tags = new List<string>();
                foreach (string tag in tags)
                {
                    // check if it exists in the db, if not add it. 
                    var tagResult = await _content.AddTag(tag);

                    // add it to the model object.
                    if (tagResult.Succeeded)
                        content.AddTag(tagResult.Item.Value);
                }
                if (content.Tags != null)
                {
                    var extraneous = content.Tags.Where(p => !tags.Select(t => t.Trim().ToTitleCase()).Any(p2 => p2 == p.TagId)).Select(s => s.TagId).ToArray();
                    for (int i = 0; i < extraneous.Count(); i++)
                        content.RemoveTag(extraneous[i]);
                }
                var _contentSettings = _settings.GetContentSettings();
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
                if (oldTemplate.IsSet() && currentTemplate.IsSet() && oldTemplate != currentTemplate)
                {
                    // delete all template metas that do not exist in the new template, and add any that are missing
                    List<string> newMetas = _content.GetMetasForTemplate(currentTemplate, type.TemplateFolder);
                    _content.UpdateTemplateMetas(content, newMetas);
                }


                OperationResult result = _content.Update(content);

                EditContentModel model = await GetEditorModel(content);
                model.Content.SaveMessage = "Saved!";
                model.Content.MessageType = AlertType.Success;
                return View(model);

            }
            catch (Exception ex)
            {
                EditContentModel model = await GetEditorModel(content);
                model.Content.SaveMessage = "There was a problem saving: " + ex.Message;
                model.Content.MessageType = AlertType.Danger;
                return View(model);
            }
        }

        [Route("admin/content/{id}/gallery/")]
        public IActionResult EditorGallery(int id)
        {
            var model = _content.GetContentByID(id);
            return View(model);
        }

        [HttpPost()]
        [Route("admin/content/designer/save/")]
        public OperationResult SaveDesigner(DesignContentModel post)
        {
            var content = _content.GetContentByID(post.Id, true);
            content.Body = post.Body;
            content.Body = Regex.Replace(content.Body, @"contenteditable=""true""", "", RegexOptions.IgnoreCase);
            content.Body = Regex.Replace(content.Body, @"id=""mce_[^;]+""", "", RegexOptions.IgnoreCase);
            content.Body = Regex.Replace(content.Body, @"mce-content-body", "", RegexOptions.IgnoreCase);
            OperationResult result = _content.Update(content);
            return result;
        }

        [Route("admin/content/{type}/create/")]
        public IActionResult Create(string type)
        {
            Content model = new Content()
            {
                PublishDate = DateTime.Now,
                Type = _settings.GetContentSettings().GetContentType(type)
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
                OperationResult result = _content.Add(model);
                if (!result.Succeeded)
                {
                    throw new Exception(result.ErrorString);
                }
                var response = new Response(true, "Published successfully.");
                response.Url = Url.Action("Edit", new { id = model.Id, message = EditorMessage.Created });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/content/{id}/categories/")]
        public IActionResult Categories(int id)
        {
            var content = _content.GetContentByID(id);
            EditContentModel model = new EditContentModel()
            {
                ContentType = _settings.GetContentSettings().GetContentType(content.ContentType),
                Content = content
            };
            return View(model);
        }

        [Route("admin/content/categories/{type}/")]
        public IActionResult CategoryList(string type)
        {
            ContentModel model = new ContentModel();
            model.ContentType = _settings.GetContentSettings().GetContentType(type);
            return View(model);
        }

        [HttpPost]
        [Route("admin/content/categories/add/")]
        public async Task<IActionResult> AddCategoryToContent(int contentId, int categoryId)
        {
            try
            {
                // User must have an organisation.
                await _content.AddCategoryToContent(contentId, categoryId);
                _categories.ResetCache();
                // If we reached here, display the organisation home.
                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [Route("admin/content/categories/remove/")]
        public IActionResult RemoveCategory(int contentId, int categoryId)
        {
            try
            {
                _content.RemoveCategoryFromContent(contentId, categoryId);
                _categories.ResetCache();
                // If we reached here, display the organisation home.
                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [HttpPost]
        [Route("admin/categories/add/")]
        public async Task<IActionResult> AddCategory(ContentCategory category)
        {
            try
            {
                // User must have an organisation.
                AccountInfo account = HttpContext.GetAccountInfo();

                // check if category is on club already
                if (!category.DisplayName.IsSet())
                    throw new Exception("You need to enter a category!");


                // check if it exists in the db, if not add it. 
                var categoryResult = await _content.AddCategory(category);
                _categories.ResetCache();
                // If we reached here, display the organisation home.
                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [Route("admin/categories/edit/{id}/")]
        public async Task<IActionResult> EditCategory(int id, string type)
        {
            var model = await _content.GetCategoryById(id);
            model.Categories = _categories.TopLevel(type);
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

                    var thisAndChildren = _categories.GetThisAndChildren(model.Id);
                    if (thisAndChildren.Select(c => c.Id).ToList().Contains(model.ParentCategoryId.Value))
                        throw new Exception("You cannot set the parent to be a child of this category!");
                }

                OperationResult result = await _content.UpdateCategory(model);
                if (result.Succeeded)
                {
                    return new Response(true);
                }
                else
                {
                    return new Response("There was a problem updating the database.");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/categories/delete/{id}/")]
        public async Task<Response> DeleteCategory(int id)
        {
            try
            {
                var result = await _content.DeleteCategory(id);
                if (result.Succeeded)
                {
                    return new Response(true);
                }
                else
                {
                    return new Response("There was a problem updating the database.");
                }
            }
            catch (Exception ex)
            {
                return new Response("Have you made sure this has no sub-categories attached to it, you cannot delete a category until you remove all the sub-categories from it");
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
                    _env.ContentRootPath + "\\Themes\\" + _settings["Hood.Settings.Theme"] + "\\Views\\Blocks\\",
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
                _env.ContentRootPath + "\\Themes\\" + _settings["Hood.Settings.Theme"] + "\\Views\\Blocks\\",
                _env.ContentRootPath + "\\Views\\Blocks\\"
            };
            string[] templateVirtualDirs = {
                "~/Themes/" + _settings["Hood.Settings.Theme"] + "/Views/Blocks/",
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
        public Response Publish(int id)
        {
            try
            {
                OperationResult<Content> result = _content.SetStatus(id, Status.Published);
                var content = _content.GetContentByID(id);
                if (result.Succeeded)
                {
                    var response = new Response(true, "Published successfully.");
                    response.Url = Url.Action("Index", new { type = content.ContentType, message = EditorMessage.Published });
                    return response;
                }
                else
                {
                    return new Response("There was a problem updating the database");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }
        [Route("admin/content/{id}/archive")]
        [HttpPost()]
        public Response Archive(int id)
        {
            try
            {
                OperationResult<Content> result = _content.SetStatus(id, Status.Archived);
                var content = _content.GetContentByID(id);
                if (result.Succeeded)
                {
                    var response = new Response(true, "Archived successfully.");
                    response.Url = Url.Action("Index", new { type= content.ContentType, message = EditorMessage.Archived });
                    return response;
                }
                else
                {
                    return new Response("There was a problem updating the database");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [HttpPost()]
        [Route("admin/content/{id}/delete")]
        public Response Delete(int id)
        {
            try
            {
                var content = _content.GetContentByID(id);
                var type = content.ContentType;
                OperationResult result = _content.Delete(id);
                if (result.Succeeded)
                {
                    var response = new Response(true, "Deleted!");
                    response.Url = Url.Action("Index", new { type = type, message = EditorMessage.Deleted });
                    return response;
                }
                else
                {
                    return new Response("There was a problem updating the database");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/content/{id}/duplicate")]
        public IActionResult Duplicate(int id)
        {
            try
            {
                var newContent = _content.DuplicateContent(id);

                return RedirectToAction("Edit", new { message = EditorMessage.Duplicated, newContent.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Edit", new { message = EditorMessage.ErrorDuplicating, id });
            }
        }

        [Route("admin/content/{id}/sethomepage")]
        public Response SetHomepage(int id)
        {
            try
            {
                var model = _settings.GetBasicSettings(false);
                model.Homepage = id;
                _settings.Set("Hood.Settings.Basic", model);
                var response = new Response(true, "The image has been cleared!");
                response.Url = Url.Action("Edit", new { id = id, message = EditorMessage.HomepageSet });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/content/{id}/upload/gallery")]
        public async Task<IActionResult> UploadToGallery(List<IFormFile> files, int id)
        {
            // User must have an organisation.
            Content content = _content.GetContentByID(id);
            if (content == null)
                return NotFound();
            AccountInfo account = HttpContext.GetAccountInfo();

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
                        await _content.AddImage(content, new ContentMedia(mediaResult));
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
        public IActionResult RemoveMedia(int id, int mediaId)
        {
            try
            {
                Content content = _content.GetContentByID(id, true);
                ContentMedia media = content.Media.Find(m => m.Id == mediaId);
                if (media != null)
                    content.Media.Remove(media);
                _content.Update(content);
            }
            catch (Exception)
            { }
            return RedirectToAction("Edit", new { id = id, message = EditorMessage.MediaRemoved });
        }

        [HttpGet]
        [Route("admin/content/{id}/media/setfeatured/{mediaId}")]
        public IActionResult SetFeatured(int id, int mediaId)
        {
            Content content = _content.GetContentByID(id, true);
            ContentMedia media = content.Media.SingleOrDefault(m => m.Id == mediaId);
            if (media != null)
            {
                content.FeaturedImage = new MediaObject(media);
                _content.Update(content);
            }
            return RedirectToAction("Edit", new { id = id, message = EditorMessage.ImageUpdated });
        }

        [Route("admin/content/getfeaturedimage/{id}")]
        public IMediaObject GetFeaturedImage(int id)
        {
            try
            {
                Content content = _content.GetContentByID(id);
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

        [Route("admin/content/getmetaimage/{id}")]
        public IMediaObject GetMetaImage(int id, string field)
        {
            try
            {
                Content content = _content.GetContentByID(id);
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
        public Response ClearImage(int id)
        {
            try
            {
                Content content = _content.GetContentByID(id);
                content.FeaturedImage = null;
                _content.Update(content);
                var response = new Response(true, "The image has been cleared!");
                response.Url = Url.Action("Edit", new { id = id, message = EditorMessage.MediaRemoved });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/content/clearmeta/{id}")]
        public Response ClearMeta(int id, string field)
        {
            try
            {
                var content = _content.GetContentByID(id);
                content.UpdateMeta(field, "");
                _content.Update(content);
                var response = new Response(true, "The data has been cleared!");
                response.Url = Url.Action("Edit", new { id = id, message = EditorMessage.ImageUpdated });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [Route("admin/content/{type}/delete/all/")]
        public async Task<IActionResult> DeleteAll(string type)
        {
            await _content.DeleteAll(type);
            return RedirectToAction("Index", new { message = EditorMessage.Deleted });
        }

        [Route("admin/content/categories/suggestions/{type}/")]
        public IActionResult CategorySuggestions(string type)
        {
            var suggestions = _categories.GetSuggestions(type).Select(c => new { id = c.Id, displayName = c.DisplayName, slug = c.Slug });
            return Json(suggestions.ToArray());
        }

        #region "Helpers"

        protected async Task<EditContentModel> GetEditorModel(Content content)
        {
            EditContentModel model = new EditContentModel()
            {
                ContentType = _settings.GetContentSettings().GetContentType(content.ContentType),
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
            var result = _settings.SubscriptionsEnabled();
            if (result.Succeeded)
            {
                // get subscriptions - if there are any.
                model.Subscriptions = await _auth.GetSubscriptionPlansAsync();
            }
            return model;
        }

        protected EditContentModel GetTemplates(EditContentModel model, string templateDirectory)
        {
            string[] templateDirs = {
                    _env.ContentRootPath + "\\Views\\" + templateDirectory + "\\",
                    _env.ContentRootPath + "\\Themes\\" + _settings["Hood.Settings.Theme"] + "\\Views\\" + templateDirectory + "\\"
                };
            List<string> templates = new List<string>();

            // Add the base templates:
            foreach (string temp in EmbeddedFiles.GetFiles("~/Views/" + templateDirectory + "/"))
            {
                if (temp.EndsWith(".cshtml"))
                    templates.Add(Path.GetFileNameWithoutExtension(temp).TrimStart('_'));
            }

            foreach (string str in templateDirs)
            {
                try
                {
                    foreach (string temp in Directory.GetFiles(str))
                    {
                        if (temp.EndsWith(".cshtml"))
                            templates.Add(Path.GetFileNameWithoutExtension(temp).TrimStart('_'));
                    }
                }
                catch { }
            }
            model.Templates = templates.Distinct().OrderBy(s => s).ToList();
            return model;
        }

        #endregion

    }
}


