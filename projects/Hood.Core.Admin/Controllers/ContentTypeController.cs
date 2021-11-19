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
    public abstract class BaseContentTypeController : BaseController
    {
        public BaseContentTypeController()
            : base()
        {
        }

        [Route("admin/content-types/manage/")]
        public virtual IActionResult Index(ContentTypeListModel model) => List(model, "Index");

        [Route("admin/content-types/list/")]
        public virtual IActionResult List(ContentTypeListModel model, string viewName = "_List_ContentType")
        {
            // get das list and shit.
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings contentSettings = Engine.Settings.Content;

            var query = contentSettings.Types.ToList();

            if (model.Search.IsSet())
            {
                query = query.Where(c =>
                    c.Title.Contains(model.Search) ||
                    c.Type.Contains(model.Search) ||
                    c.Type.Contains(model.Search) ||
                    c.TypeName.Contains(model.Search)
                ).ToList();
            }

            switch (model.Order)
            {
                case "Title":
                    query = query.OrderBy(n => n.Title).ToList();
                    break;

                case "TitleDesc":
                    query = query.OrderByDescending(n => n.Title).ToList();
                    break;

                default:
                    query = query.OrderBy(n => n.Title).ToList();
                    break;
            }

            model.Reload(query);

            return View(viewName, model);
        }

        #region Fields 

        [Route("admin/content-types/{id}/fields")]
        public virtual IActionResult FieldList(string id, string viewName = "_List_CustomField")
        {
            var model = Engine.Settings.Content.GetContentType(id);
            return View(viewName, model);
        }

        [Route("admin/content-types/{id}/fields/add")]
        public virtual IActionResult CreateField()
        {
            return View("_Blade_CustomField");
        }

        [HttpPost]
        [Route("admin/content-types/{id}/fields/add")]
        public virtual async Task<Response> CreateField(string id, CustomField model)
        {
            try
            {
                var contentType = Engine.Settings.Content.GetContentType(id);

                // add the field.
                if (!model.Name.IsSet())
                {
                    throw new Exception("You must enter a field name.");
                }

                model.Name = $"Custom.{contentType.Type.ToTitleCase()}.{model.Name}";
                model.System = false;

                var fields = contentType.CustomFields;
                fields.Add(model);
                contentType.CustomFields = fields;

                ContentType modelToUpdate = SaveContentType(contentType, id);

                return new Response(true, $"The field was added successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseContentTypeController>($"Error adding custom field.", ex);
            }
        }

        [HttpPost]
        [Route("admin/content-types/{id}/fields/delete")]
        public virtual async Task<Response> DeleteField(string id, string name)
        {
            try
            {
                var contentType = Engine.Settings.Content.GetContentType(id);

                var fields = contentType.CustomFields;
                fields.RemoveAll(f => f.Name == name);
                contentType.CustomFields = fields;

                ContentType modelToUpdate = SaveContentType(contentType, id);

                return new Response(true, $"The field was removed successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseContentTypeController>($"Error removing custom field.", ex);
            }
        }

        #endregion

        #region Create

        [Route("admin/content-types/create")]
        public virtual IActionResult Create()
        {
            return View("_Blade_ContentType");
        }

        [HttpPost]
        [Route("admin/content-types/create")]
        public virtual async Task<Response> Create(ContentType model)
        {
            try
            {
                _cache.Remove(typeof(ContentSettings).ToString());
                ContentSettings contentSettings = Engine.Settings.Content;
                if (contentSettings == null)
                    contentSettings = new ContentSettings();
                var types = contentSettings.Types.ToList();

                if (types.Any(t => t.Type == model.Type))
                {
                    throw new Exception("The type you have entered is already being used.");
                }
                if (types.Any(t => t.Slug == model.Slug))
                {
                    throw new Exception("The slug you have entered is already being used.");
                }

                types.Add(model);

                contentSettings.Types = types.ToArray();
                contentSettings.CheckBaseFields();

                Engine.Settings.Set(contentSettings);

                // refresh all content metas and things
                await RefreshAllMetasAsync();

                MessageType = AlertType.Success;
                SaveMessage = "Created successfully.";

                return new Response(true, $"The content was created successfully.<br /><a href='{Url.Action(nameof(Edit), new { type = model.Type })}'>Go to the new content type</a>");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseContentTypeController>($"Error publishing content type.", ex);
            }
        }

        protected virtual async Task RefreshAllMetasAsync()
        {
            foreach (var content in _db.Content.Include(p => p.Metadata).AsNoTracking().ToList())
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

        #endregion

        #region Edit

        [Route("admin/content-types/{id}/edit/")]
        public virtual IActionResult Edit(string id)
        {
            var model = Engine.Settings.Content.GetContentType(id);
            return View(model);
        }

        [HttpPost()]
        [Route("admin/content-types/{id}/edit/")]
        public virtual async Task<ActionResult> Edit(ContentType model, string id)
        {
            try
            {
                var modelToUpdate = Engine.Settings.Content.GetContentType(id);

                var updatedFields = Request.Form.Keys.ToHashSet();
                modelToUpdate = modelToUpdate.UpdateFromFormModel(model, updatedFields);

                modelToUpdate = SaveContentType(model, id);
                if (model.Type != id)
                {
                    return RedirectToAction(nameof(Edit), new { id = model.Type });
                }
                return View(modelToUpdate);
            }
            catch (Exception ex)
            {
                SaveMessage = "There was a problem saving: " + ex.Message;
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<BaseContentTypeController>(SaveMessage, ex);
            }

            return View(model);
        }

        protected virtual ContentType SaveContentType(ContentType model, string id)
        {
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings contentSettings = Engine.Settings.Content;

            if (contentSettings == null)
                contentSettings = new ContentSettings();

            var types = contentSettings.Types.ToList();

            types.RemoveAll(t => t.Type.ToLower() == id);

            types.Add(model);

            contentSettings.Types = types.ToArray();

            contentSettings.CheckBaseFields();

            Engine.Settings.Set(contentSettings);

            return model;
        }

        #endregion


        [Route("admin/content-types/{id}/delete/")]
        public virtual async Task<IActionResult> Delete(string id)
        {
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings model = Engine.Settings.Content;

            if (model == null)
                model = new ContentSettings();

            var types = model.Types.ToList();

            if (!types.Any(t => t.Type.ToLower() == id))
            {
                MessageType = AlertType.Info;
                SaveMessage = "Deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            types.RemoveAll(t => t.Type.ToLower() == id);

            model.Types = types.ToArray();

            model.CheckBaseFields();

            Engine.Settings.Set(model);

            // refresh all content metas and things
            await RefreshAllMetasAsync();

            MessageType = AlertType.Info;
            SaveMessage = "Deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Route("admin/settings/content/reset/")]
        public virtual async Task<IActionResult> ResetContent()
        {
            var model = new ContentSettings();

            Engine.Settings.Set(model);

            // refresh all content metas and things
            await RefreshAllMetasAsync();

            SaveMessage = $"The settings have been reset to their default values.";
            MessageType = AlertType.Success;

            return RedirectToAction(nameof(Index));
        }


    }
}


