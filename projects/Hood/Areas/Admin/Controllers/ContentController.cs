using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.IO;
using Hood.Models;
using Hood.Models.Api;
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
        private readonly IMediaManager<SiteMedia> _media;
        private readonly IAccountRepository _auth;

        public ContentController(
            IAccountRepository auth,
            IContentRepository content,
            ContentCategoryCache categories,
            UserManager<ApplicationUser> userManager,
            ISettingsRepository site,
            IBillingService billing,
            IMediaManager<SiteMedia> media,
            IHostingEnvironment env)
        {
            _auth = auth;
            _media = media;
            _userManager = userManager;
            _content = content;
            _settings = site;
            _categories = categories;
            _env = env;
        }

        [Route("admin/content/{type}/manage/")]
        public IActionResult Index(string type)
        {
            var contentType = _settings.GetContentSettings().GetContentType(type);
            ContentListModel nhm = new ContentListModel()
            {
                Type = contentType
            };
            return View(nhm);
        }

        [Route("admin/content/{id}/edit/")]
        public async Task<IActionResult> Edit(int id)
        {
            var content = _content.GetContentByID(id, true);
            if (content == null)
                return NotFound();
            EditContentModel model = await GetEditorModel(content);
            return View(model);
        }

        [HttpPost()]
        [Route("admin/content/{id}/edit/")]
        public async Task<ActionResult> Edit(EditContentModelSend post)
        {
            var model = await Save(post, false);
            return View(model);
        }

        [Route("admin/content/{id}/gallery/")]
        public IActionResult EditorGallery(int id)
        {
            var content = _content.GetContentByID(id);
            var model = _settings.ToContentApi(content);
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
            ContentListModel mcm = new ContentListModel()
            {
                Type = _settings.GetContentSettings().GetContentType(type)
            };
            return View(mcm);
        }

        [HttpPost]
        public async Task<Response> Add(CreateContentModel model)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);
                Content content = new Content
                {
                    AllowComments = true,
                    AuthorId = user.Id,
                    Body = "",
                    CreatedBy = user.UserName,
                    CreatedOn = DateTime.Now,
                    Excerpt = model.cpExcept,
                    LastEditedBy = user.UserName,
                    LastEditedOn = DateTime.Now,
                    ContentType = model.cpType,
                    Public = true,
                    ShareCount = 0,
                    PublishDate = new DateTime(model.cpPublishDate.Year, model.cpPublishDate.Month, model.cpPublishDate.Day, model.cpPublishHour, model.cpPublishMinute, 0),
                    Status = model.cpStatus,
                    Title = model.cpTitle,
                    Views = 0
                };
                OperationResult result = _content.Add(content);
                if (!result.Succeeded)
                {
                    throw new Exception(result.ErrorString);
                }
                if (model.cpCategory.IsSet())
                {
                    var categoryResult = await _content.AddCategory(model.cpCategory, content.ContentType);
                    if (content.Categories == null)
                        content.Categories = new List<ContentCategoryJoin>();
                    // add it to the model object.
                    if (categoryResult.Succeeded)
                        content.Categories.Add(new ContentCategoryJoin() { CategoryId = categoryResult.Item.ContentCategoryId, ContentId = content.Id });
                    _content.Update(content);
                }
                return new Response(true);

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
                Type = _settings.GetContentSettings().GetContentType(content.ContentType),
                Content = content
            };
            return View(model);
        }

        [Route("admin/content/categories/{type}/")]
        public IActionResult CategoryList(string type)
        {
            var contentType = _settings.GetContentSettings().GetContentType(type);
            ContentListModel nhm = new ContentListModel()
            {
                Type = contentType
            };
            return View(nhm);
        }

        [HttpPost]
        [Route("admin/content/{id}/categories/add/")]
        public async Task<IActionResult> AddCategoryToContent(int id, string category)
        {
            try
            {
                // User must have an organisation.
                Content content = _content.GetContentByID(id, true);
                if (content == null)
                    return NotFound();
                AccountInfo account = HttpContext.GetAccountInfo();

                // check if category is on club already
                if (!category.IsSet())
                    throw new Exception("You need to enter a category!");
                if (content.IsInCategory(category.Trim().ToTitleCase()))
                    throw new Exception("The content is already in the " + category + " category.");


                // check if it exists in the db, if not add it. 
                var categoryResult = await _content.AddCategory(category, content.ContentType);
                if (content.Categories == null)
                    content.Categories = new List<ContentCategoryJoin>();
                // add it to the model object.
                if (categoryResult.Succeeded)
                    content.Categories.Add(new ContentCategoryJoin() { CategoryId = categoryResult.Item.ContentCategoryId, ContentId = content.Id });

                var contentResult = _content.Update(content);
                _categories.ResetCache();
                // If we reached here, display the organisation home.
                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        [Route("admin/content/{id}/categories/remove/")]
        public IActionResult RemoveCategory(int id, string category)
        {
            try
            {
                // User must have an organisation.
                Content content = _content.GetContentByID(id, true);
                if (content == null)
                    return NotFound();

                // check if category is on club already
                if (!content.IsInCategory(category))
                    throw new Exception("The club is not in the " + category + " category.");

                var cc = content.Categories.Find(m => m.Category.DisplayName == category);
                content.Categories.Remove(cc);
                var clubResult = _content.Update(content);
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
                    if (model.ParentCategoryId == model.ContentCategoryId)
                        throw new Exception("You cannot set the parent to be the same category!");

                    var thisAndChildren = _categories.GetThisAndChildren(model.ContentCategoryId);
                    if (thisAndChildren.Select(c => c.ContentCategoryId).ToList().Contains(model.ParentCategoryId.Value))
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
                return new Response(ex.Message);
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
                if (result.Succeeded)
                {
                    return new Response(true);
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
                if (result.Succeeded)
                {
                    return new Response(true);
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
                OperationResult result = _content.Delete(id);
                if (result.Succeeded)
                {
                    return new Response(true);
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

        [Route("admin/content/{id}/sethomepage")]
        public Response SetHomepage(int id)
        {
            try
            {
                var model = _settings.GetBasicSettings(false);
                model.Homepage = id;
                _settings.Set("Hood.Settings.Basic", model);
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        /// <summary>
        /// This adds images to the club gallery.
        /// </summary>
        /// <param name="clubSlug">The SEO slug for the club</param>
        /// <returns></returns>
        [Authorize]
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
                SiteMedia mediaResult = null;
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
                        mediaResult = await _media.ProcessUpload(file, new SiteMedia() { Directory = content.ContentType.ToTitleCase() });
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
            return RedirectToAction("Edit", new { id = id });
        }

        [HttpGet]
        [Route("admin/content/{id}/media/setfeatured/{mediaId}")]
        public IActionResult SetFeatured(int id, int mediaId)
        {
            try
            {
                Content content = _content.GetContentByID(id, true);
                ContentMedia media = content.Media.SingleOrDefault(m => m.Id == mediaId);
                if (media != null)
                {
                    content.FeaturedImage = new SiteMedia(media);
                    _content.Update(content);
                }
            }
            catch (Exception)
            { }
            return RedirectToAction("Edit", new { id = id });
        }

        public JsonResult Get(ListFilters request, string search, string sort, string type, string category, bool published = false)
        {
            PagedList<Content> content = _content.GetPagedContent(request, type, category, null, null, published);

            Response response = new Response(content.Items.Select(c => _settings.ToContentApi(c)).ToArray(), content.Count);
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return Json(response, settings);
        }

        public JsonResult GetById(int id)
        {
            IList<Content> content = new List<Content>
            {
                _content.GetContentByID(id)
            };
            var ups = content.Select(c => _settings.ToContentApi(c));
            return Json(ups.ToArray());
        }

        public MediaApi GetFeaturedImage(int id)
        {
            try
            {
                Content content = _content.GetContentByID(id);
                if (content != null && content.FeaturedImage != null)
                    return new MediaApi(content.FeaturedImage, _settings);
                else
                    throw new Exception("No featured image found");
            }
            catch (Exception)
            {
                return MediaApi.Blank(_settings.GetMediaSettings());
            }
        }

        public MediaApi GetMetaImage(int id, string field)
        {
            try
            {
                Content content = _content.GetContentByID(id);
                if (content != null)
                    return new MediaApi(content.GetMeta(field).Get<IMediaObject>(), _settings);
                else
                    throw new Exception("No featured image found");
            }
            catch (Exception)
            {
                return MediaApi.Blank(_settings.GetMediaSettings());
            }
        }

        public Response ClearImage(int id)
        {
            try
            {
                Content content = _content.GetContentByID(id);
                content.FeaturedImage = null;
                _content.Update(content);
                return new Response(true, "The image has been cleared!");
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        public Response ClearMeta(int id, string field)
        {
            try
            {
                var content = _content.GetContentByID(id);
                content.UpdateMeta(field, "");
                _content.Update(content);
                return new Response(true, "Cleared!");
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
            return RedirectToAction("Index");
        }

        public IActionResult CategorySuggestions(string query)
        {
            IEnumerable<string> suggestions = _categories.GetSuggestions(query);
            return Json(suggestions.ToArray());
        }

        //public IActionResult TagSuggestions(string query)
        //{
        //}

        #region "Helpers"

        private async Task<EditContentModel> Save(EditContentModelSend post, bool retainBody)
        {
            var content = _content.GetContentByID(post.Id, true);
            try
            {
                if (retainBody)
                    post.Body = content.Body;
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
                model.SaveResult = result;
                return model;

            }
            catch (Exception ex)
            {
                EditContentModel model = await GetEditorModel(content);
                model.SaveResult = new OperationResult(ex.Message);
                return model;
            }
        }

        private async Task<EditContentModel> GetEditorModel(Content content)
        {
            EditContentModel model = new EditContentModel()
            {
                Type = _settings.GetContentSettings().GetContentType(content.ContentType),
                Content = content
            };
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var editors = await _userManager.GetUsersInRoleAsync("Editor");
            model.Authors = editors.Concat(admins).Distinct().OrderBy(u => u.FirstName).ThenBy(u => u.Email).ToList();

            // Templates
            if (model.Type.Templates)
                model = GetTemplates(model, model.Type.TemplateFolder);

            // Special type features.
            switch (model.Type.BaseName)
            {
                case "Page":
                    model = await GetPageEditorFeatures(model);
                    break;
            }

            return model;
        }

        private async Task<EditContentModel> GetPageEditorFeatures(EditContentModel model)
        {
            var result = _settings.SubscriptionsEnabled();
            if (result.Succeeded)
            {
                // get subscriptions - if there are any.
                model.Subscriptions = await _auth.GetSubscriptionPlansAsync();
            }
            return model;
        }

        private EditContentModel GetTemplates(EditContentModel model, string templateDirectory)
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


