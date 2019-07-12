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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]
    public class SettingsController : BaseController
    {
        protected IMediaRefreshService _mediaRefresh;

        public SettingsController(
            IMediaRefreshService mediaRefresh
            )
            : base()
        {
            _mediaRefresh = mediaRefresh;
        }

        [Route("admin/settings/basics/")]
        public IActionResult Basics()
        {
            _cache.Remove(typeof(BasicSettings).ToString());
            BasicSettings model = Engine.Settings.Basic;
            if (model == null)
                model = new BasicSettings();
            return View(model);
        }

        [HttpPost]
        [Route("admin/settings/basics/")]
        public IActionResult Basics(BasicSettings model)
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
        public IActionResult ResetBasics()
        {
            var model = new BasicSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Basics");
        }



        [Route("admin/integrations/")]
        public IActionResult Integrations()
        {
            _cache.Remove(typeof(IntegrationSettings).ToString());
            IntegrationSettings model = Engine.Settings.Integrations;
            if (model == null)
                model = new IntegrationSettings();
            return View(model);
        }

        [HttpPost]
        [Route("admin/integrations/")]
        public IActionResult Integrations(IntegrationSettings model)
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
        public IActionResult ResetIntegrations()
        {
            var model = new IntegrationSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Integrations");
        }


        [Route("admin/settings/contact/")]
        public IActionResult Contact()
        {
            _cache.Remove(typeof(ContactSettings).ToString());
            ContactSettings model = Engine.Settings.Contact;
            if (model == null)
                model = new ContactSettings();
            return View(model);
        }

        [HttpPost]
        [Route("admin/settings/contact/")]
        public IActionResult Contact(ContactSettings model)
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
        public IActionResult ResetContact()
        {
            var model = new ContactSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Contact");
        }


        [Route("admin/settings/content/")]
        public IActionResult Content()
        {
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings model = Engine.Settings.Content;
            if (model == null)
                model = new ContentSettings();
            return View(model);
        }

        [HttpPost]
        [Route("admin/settings/content/")]
        public async Task<IActionResult> Content(ContentSettings model)
        {
            try
            {
                model.CheckBaseFields();

                Engine.Settings.Set(model);

                // refresh all content metas and things
                await _content.RefreshAllMetasAsync();


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

        [Route("admin/settings/content/reset/")]
        public async Task<IActionResult> ResetContent()
        {
            var model = new ContentSettings();
            Engine.Settings.Set(model);
            // refresh all content metas and things
            await _content.RefreshAllMetasAsync();
            return RedirectWithResetMessage("Content");
        }

        [Route("admin/settings/content/add-type/")]
        public async Task<IActionResult> AddContentType()
        {
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings model = Engine.Settings.Content;
            if (model == null)
                model = new ContentSettings();
            var types = model.Types.ToList();
            if (types.Any(t => t.Type == "new-content"))
            {
                SaveMessage = "Could not add new content type, as you need to customise the last one you added first.";
                MessageType = AlertType.Danger;
                return RedirectToAction("Content");
            }
            types.Add(new ContentType()
            {
                BaseName = "Default",
                Slug = "new-content",
                Type = "new-content",
                Search = "new-content",
                Title = "New Content Type",
                Icon = "fa-object-group",
                TypeName = "Slider",
                Enabled = true,
                IsPublic = false,
                HasPage = false,
                TypeNamePlural = "New Content Types",
                TitleName = "Title",
                ExcerptName = "Excerpt",
                MultiLineExcerpt = true,
                ShowDesigner = false,
                ShowEditor = true,
                ShowCategories = true,
                ShowBanner = true,
                ShowImage = true,
                Gallery = false,
                Templates = false,
                TemplateFolder = "Templates",
                CustomFields = ContentTypes.BaseFields(new List<CustomField>())
            });
            model.Types = types.ToArray();
            model.CheckBaseFields();

            Engine.Settings.Set(model);

            // refresh all content metas and things
            await _content.RefreshAllMetasAsync();

            MessageType = AlertType.Success;
            SaveMessage = "Created successfully.";

            return RedirectToAction("Content");
        }

        [Route("admin/settings/content/delete-type/")]
        public async Task<IActionResult> DeleteContentType(string type)
        {
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings model = Engine.Settings.Content;

            if (model == null)
                model = new ContentSettings();

            var types = model.Types.ToList();
            if (!types.Any(t => t.Type == type))
            {
                MessageType = AlertType.Info;
                SaveMessage = "Deleted successfully.";
                return RedirectToAction("Content");
            }
            types.RemoveAll(t => t.Type == type);
            model.Types = types.ToArray();
            model.CheckBaseFields();

            Engine.Settings.Set(model);

            // refresh all content metas and things
            await _content.RefreshAllMetasAsync();

            MessageType = AlertType.Info;
            SaveMessage = "Deleted successfully.";

            return RedirectToAction("Content");
        }

        [Route("admin/settings/property/")]
        public IActionResult Property()
        {
            _cache.Remove(typeof(PropertySettings).ToString());
            PropertySettings model = Engine.Settings.Property;
            if (model == null)
                model = new PropertySettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/property/")]
        public IActionResult Property(PropertySettings model)
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
        public IActionResult ResetProperty()
        {
            var model = new PropertySettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Property");
        }


        [Route("admin/settings/billing/")]
        public IActionResult Billing()
        {
            _cache.Remove(typeof(BillingSettings).ToString());
            BillingSettings model = Engine.Settings.Billing;
            if (model == null)
                model = new BillingSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/billing/")]
        public IActionResult Billing(BillingSettings model)
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

        [Route("admin/settings/billing/reset/")]
        public IActionResult ResetBilling()
        {
            var model = new BillingSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Billing");
        }

        [Route("admin/settings/account/")]
        public IActionResult AccountSettings()
        {
            _cache.Remove(typeof(AccountSettings).ToString());
            AccountSettings model = Engine.Settings.Account;
            if (model == null)
                model = new AccountSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/account/")]
        public IActionResult AccountSettings(AccountSettings model)
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
        public IActionResult ResetAccount()
        {
            var model = new AccountSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Account");
        }


        [Route("admin/settings/seo/")]

        public IActionResult Seo()
        {
            _cache.Remove(typeof(SeoSettings).ToString());
            SeoSettings model = Engine.Settings.Seo;
            if (model == null)
                model = new SeoSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/seo/")]
        public IActionResult Seo(SeoSettings model)
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
        public IActionResult ResetSeo()
        {
            var model = new SeoSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Seo");
        }


        [Route("admin/settings/media/")]
        public IActionResult Media()
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
        public IActionResult Media(MediaSettings model)
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
        public IActionResult ResetMedia()
        {
            var model = new MediaSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Media");
        }
        [Route("admin/settings/media/refresh/")]
        public IActionResult RefreshMedia()
        {
            _mediaRefresh.Kill();
            _mediaRefresh.RunUpdate(HttpContext);

            SaveMessage = "Media refreshing...";
            MessageType = AlertType.Success;

            return RedirectToAction("Media");
        }
        [HttpPost]
        [Route("admin/settings/media/refresh/cancel/")]
        public IActionResult RefreshMediaKillAsync()
        {
            _mediaRefresh.Kill();
            return Json(new { success = true });
        }

        [Route("admin/settings/mail/")]
        public IActionResult Mail()
        {
            _cache.Remove(typeof(MailSettings).ToString());
            MailSettings model = Engine.Settings.Mail;
            if (model == null)
                model = new MailSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/mail/")]
        public IActionResult Mail(MailSettings model)
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
        public IActionResult ResetMail()
        {
            var model = new MailSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Mail");
        }

        [Route("admin/settings/forum/")]
        public async Task<IActionResult> Forum()
        {
            _cache.Remove(typeof(ForumSettings).ToString());
            ForumSettings model = Engine.Settings.Forum;
            if (model == null)
                model = new ForumSettings();

            var subs = await _account.GetSubscriptionPlansAsync(new SubscriptionSearchModel() { PageSize = int.MaxValue });
            model.Subscriptions = subs.List;

            model.Roles = await _account.GetAllRolesAsync();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/forum/")]
        public async Task<IActionResult> Forum(ForumSettings model)
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
            var subs = await _account.GetSubscriptionPlansAsync(new SubscriptionSearchModel() { PageSize = int.MaxValue });
            model.Subscriptions = subs.List;

            model.Roles = await _account.GetAllRolesAsync();
            return View(model);
        }

        [Route("admin/settings/forum/reset/")]
        public IActionResult ResetForum()
        {
            var model = new ForumSettings();
            Engine.Settings.Set(model);
            return RedirectWithResetMessage("Forum");
        }


        [Route("admin/settings/advanced/")]
        public IActionResult Advanced()
        {
            return View(new SaveableModel());
        }

        #region Caching 
        [Route("admin/settings/removecacheitem/")]
        public IActionResult RemoveCacheItem(string key)
        {
            _cache.Remove(key);
            SaveMessage = $"The item {key} has been cleared from the cache.";
            MessageType = AlertType.Success;
            return RedirectToAction("Advanced");
        }
        [Route("admin/settings/resetcache/")]
        public IActionResult ResetCache()
        {
            _cache.ResetCache();
            SaveMessage = $"The site cache has been cleared.";
            MessageType = AlertType.Success;
            return RedirectToAction("Advanced");
        }
        #endregion

        #region Helpers 
        public IActionResult RedirectWithResetMessage(string actionName)
        {
            SaveMessage = $"The settings have been reset to their default values.";
            MessageType = AlertType.Success;
            return RedirectToAction(actionName);
        }
        #endregion
    }
}
