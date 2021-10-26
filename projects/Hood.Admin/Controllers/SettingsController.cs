using Geocoding.Google;
using Hood.BaseTypes;
using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
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
    [Authorize(Roles = "SuperUser,Admin")]
    public class SettingsController : BaseSettingsController
    {
        public SettingsController()
            : base()
        {
        }
    }

    public abstract class BaseSettingsController : BaseController
    {
        protected IMediaRefreshService _mediaRefresh;
        public BaseSettingsController()
            : base()
        {
            _mediaRefresh = Engine.Services.Resolve<IMediaRefreshService>();
        }

        [Route("admin/settings/basics/")]
        public virtual IActionResult Basics()
        {
            _cache.Remove(typeof(BasicSettings).ToString());
            BasicSettings model = Engine.Settings.Basic;
            if (model == null)
                model = new BasicSettings();
            return View(model);
        }

        [HttpPost]
        [Route("admin/settings/basics/")]
        public virtual IActionResult Basics(BasicSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
                if (model.Address.IsSet())
                {
                    var location = _address.GeocodeAddress(model.Address);
                    if (location != null)
                    {
                        model.Address.Latitude = location.Coordinates.Latitude;
                        model.Address.Longitude = location.Coordinates.Longitude;
                        Engine.Settings.Set(model);
                        SaveMessage = "Settings saved & address geocoded via Google API.";
                        MessageType = AlertType.Success;
                    }
                    else
                    {
                        SaveMessage = "Settings were saved, but because there was an error with the Google API, your address could not be located on the map. Check your Google API key in your Integration Settings, and ensure your API key has the Geocoding API enabled.";
                        MessageType = AlertType.Warning;
                    }
                }
                else
                {
                    SaveMessage = "Settings were saved, but you did not set an address correctly, so the Google API could not locate your address on the map.";
                    MessageType = AlertType.Warning;
                }

            }
            catch (GoogleGeocodingException ex)
            {
                switch (ex.Status)
                {
                    case GoogleStatus.RequestDenied:
                        SaveMessage = "Settings were saved, but because there was an error with the Google API [Google returned a RequestDenied status] this means your API account is not activated for Geocoding Requests.";
                        MessageType = AlertType.Warning;
                        break;
                    case GoogleStatus.OverQueryLimit:
                        SaveMessage = "Settings were saved, but because there was an error with the Google API [Google returned a OverQueryLimit status] this means your API account is has run out of Geocoding Requests.";
                        MessageType = AlertType.Warning;
                        break;
                    default:
                        SaveMessage = "Settings were saved, but because there was an error with the Google API, your address could not be located on the map. Check your Google API key in your Integration Settings, and ensure your API key has the Geocoding API enabled. Google returned a status of " + ex.Status.ToString();
                        MessageType = AlertType.Warning;
                        break;
                }
            }
            catch (Exception ex)
            {
                SaveMessage = "Error saving: " + ex.Message;
                MessageType = AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/basics/reset/")]
        public virtual IActionResult ResetBasics()
        {
            var model = new BasicSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Basics");
        }

        [Route("admin/integrations/")]
        public virtual IActionResult Integrations()
        {
            _cache.Remove(typeof(IntegrationSettings).ToString());
            IntegrationSettings model = Engine.Settings.Integrations;
            if (model == null)
                model = new IntegrationSettings();
            return View(model);
        }

        [HttpPost]
        [Route("admin/integrations/")]
        public virtual IActionResult Integrations(IntegrationSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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

        [Route("admin/settings/integrations/reset/")]
        public virtual IActionResult ResetIntegrations()
        {
            var model = new IntegrationSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Integrations");
        }


        [Route("admin/settings/contact/")]
        public virtual IActionResult Contact()
        {
            _cache.Remove(typeof(ContactSettings).ToString());
            ContactSettings model = Engine.Settings.Contact;
            if (model == null)
                model = new ContactSettings();
            return View(model);
        }

        [HttpPost]
        [Route("admin/settings/contact/")]
        public virtual IActionResult Contact(ContactSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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

        [Route("admin/settings/contact/reset/")]
        public virtual IActionResult ResetContact()
        {
            var model = new ContactSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Contact");
        }


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


        [Route("admin/settings/property/")]
        public virtual IActionResult Property()
        {
            _cache.Remove(typeof(PropertySettings).ToString());
            PropertySettings model = Engine.Settings.Property;
            if (model == null)
                model = new PropertySettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/property/")]
        public virtual IActionResult Property(PropertySettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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

        [Route("admin/settings/property/reset/")]
        public virtual IActionResult ResetProperty()
        {
            var model = new PropertySettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Property");
        }

        [Route("admin/settings/account/")]
        public virtual IActionResult AccountSettings()
        {
            _cache.Remove(typeof(AccountSettings).ToString());
            AccountSettings model = Engine.Settings.Account;
            if (model == null)
                model = new AccountSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/account/")]
        public virtual IActionResult AccountSettings(AccountSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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

        [Route("admin/settings/account/reset/")]
        public virtual IActionResult ResetAccount()
        {
            var model = new AccountSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Account");
        }


        [Route("admin/settings/seo/")]

        public virtual IActionResult Seo()
        {
            _cache.Remove(typeof(SeoSettings).ToString());
            SeoSettings model = Engine.Settings.Seo;
            if (model == null)
                model = new SeoSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/seo/")]
        public virtual IActionResult Seo(SeoSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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

        [Route("admin/settings/seo/reset/")]
        public virtual IActionResult ResetSeo()
        {
            var model = new SeoSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Seo");
        }


        [Route("admin/settings/media/")]
        public virtual IActionResult Media()
        {
            _cache.Remove(typeof(MediaSettings).ToString());
            MediaSettings model = Engine.Settings.Media;
            if (model == null)
                model = new MediaSettings();

            model.UpdateReport = _mediaRefresh.Report();

            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/media/")]
        public virtual IActionResult Media(MediaSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
                SaveMessage = "Settings saved!";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "Error saving: " + ex.Message;
                MessageType = AlertType.Danger;
            }

            model.UpdateReport = _mediaRefresh.Report();

            return View(model);
        }

        [Route("admin/settings/media/reset/")]
        public virtual IActionResult ResetMedia()
        {
            var model = new MediaSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Media");
        }
        [Route("admin/settings/media/refresh/")]
        public virtual IActionResult RefreshMedia()
        {
            _mediaRefresh.Kill();
            _mediaRefresh.RunUpdate(HttpContext);

            SaveMessage = "Media refreshing...";
            MessageType = AlertType.Success;

            return RedirectToAction("Media");
        }
        [HttpPost]
        [Route("admin/settings/media/refresh/cancel/")]
        public virtual IActionResult RefreshMediaKillAsync()
        {
            _mediaRefresh.Kill();
            return Json(new { success = true });
        }

        [Route("admin/settings/mail/")]
        public virtual IActionResult Mail()
        {
            _cache.Remove(typeof(MailSettings).ToString());
            MailSettings model = Engine.Settings.Mail;
            if (model == null)
                model = new MailSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/mail/")]
        public virtual IActionResult Mail(MailSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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

        [Route("admin/settings/mail/reset/")]
        public virtual IActionResult ResetMail()
        {
            var model = new MailSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Mail");
        }


        [Route("admin/settings/advanced/")]
        public virtual IActionResult Advanced()
        {
            return View(new SaveableModel());
        }

        #region Caching 
        [Route("admin/settings/removecacheitem/")]
        public virtual IActionResult RemoveCacheItem(string key)
        {
            _cache.Remove(key);
            SaveMessage = $"The item {key} has been cleared from the cache.";
            MessageType = AlertType.Success;
            return RedirectToAction("Advanced");
        }
        [Route("admin/settings/resetcache/")]
        public virtual IActionResult ResetCache()
        {
            _cache.ResetCache();
            SaveMessage = $"The site cache has been cleared.";
            MessageType = AlertType.Success;
            return RedirectToAction("Advanced");
        }
        #endregion

        #region Helpers 
        public virtual IActionResult RedirectWithResetMessage(string actionName)
        {
            SaveMessage = $"The settings have been reset to their default values.";
            MessageType = AlertType.Success;
            return RedirectToAction(actionName);
        }

        public virtual async Task RefreshAllMetasAsync()
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
    }
}
