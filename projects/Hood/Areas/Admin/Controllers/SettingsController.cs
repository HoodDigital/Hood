using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Hood.Services;
using Hood.Models;
using System;
using Hood.Caching;
using Geocoding.Google;
using Hood.Enums;
using System.Threading.Tasks;
using Hood.Extensions;
using Hood.Controllers;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Basics()
        {
            BasicSettings model = _settings.GetBasicSettings(true);
            if (model == null)
                model = new BasicSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/basics/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Basics(BasicSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Basic", model);
                var location = _address.GeocodeAddress(model.Address);
                if (location != null)
                {
                    model.Address.Latitude = location.Coordinates.Latitude;
                    model.Address.Longitude = location.Coordinates.Longitude;
                    _settings.Set("Hood.Settings.Basic", model);
                    model.SaveMessage = "Settings saved & address geocoded via Google API.";
                    model.MessageType = Enums.AlertType.Success;
                }
                else
                {
                    model.SaveMessage = "Settings were saved, but because there was an error with the Google API, your address could not be located on the map. Check your Google API key in your Integration Settings, and ensure your API key has the Geocoding API enabled.";
                    model.MessageType = Enums.AlertType.Warning;
                }
            }
            catch (GoogleGeocodingException ex)
            {
                switch (ex.Status)
                {
                    case GoogleStatus.RequestDenied:
                        model.SaveMessage = "Settings were saved, but because there was an error with the Google API [Google returned a RequestDenied status] this means your API account is not activated for Geocoding Requests.";
                        model.MessageType = Enums.AlertType.Warning;
                        break;
                    case GoogleStatus.OverQueryLimit:
                        model.SaveMessage = "Settings were saved, but because there was an error with the Google API [Google returned a OverQueryLimit status] this means your API account is has run out of Geocoding Requests.";
                        model.MessageType = Enums.AlertType.Warning;
                        break;
                    default:
                        model.SaveMessage = "Settings were saved, but because there was an error with the Google API, your address could not be located on the map. Check your Google API key in your Integration Settings, and ensure your API key has the Geocoding API enabled. Google returned a status of " + ex.Status.ToString();
                        model.MessageType = Enums.AlertType.Warning;
                        break;
                }
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/basics/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetBasics()
        {
            var model = new BasicSettings();
            _settings.Set("Hood.Settings.Basic", model);
            return RedirectToAction("Basics");
        }


        [Route("admin/integrations/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Integrations()
        {
            IntegrationSettings model = _settings.GetIntegrationSettings(true);
            if (model == null)
                model = new IntegrationSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/integrations/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Integrations(IntegrationSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Integrations", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/integrations/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetIntegrations()
        {
            var model = new IntegrationSettings();
            _settings.Set("Hood.Settings.Integrations", model);
            return RedirectToAction("Integrations");
        }


        [Route("admin/settings/contact/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Contact()
        {
            ContactSettings model = _settings.GetContactSettings(true);
            if (model == null)
                model = new ContactSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/contact/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Contact(ContactSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Contact", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/contact/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetContact()
        {
            var model = new ContactSettings();
            _settings.Set("Hood.Settings.Contact", model);
            return RedirectToAction("Contact");
        }


        [Route("admin/settings/content/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Content(EditorMessage? message)
        {
            ContentSettings model = _settings.GetContentSettings(true);
            if (model == null)
                model = new ContentSettings();
            model.AddEditorMessage(message);
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/content/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Content(ContentSettings model)
        {
            try
            {
                model.CheckBaseFields();

                _settings.Set("Hood.Settings.Content", model);

                // refresh all content metas and things
                _content.RefreshAllMetas();


                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/content/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetContent()
        {
            var model = new ContentSettings();
            _settings.Set("Hood.Settings.Content", model);
            // refresh all content metas and things
            _content.RefreshAllMetas();
            return RedirectToAction("Content");
        }
        [Route("admin/settings/content/add-type/")]

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AddContentType()
        {
            ContentSettings model = _settings.GetContentSettings(true);
            if (model == null)
                model = new ContentSettings();
            var types = model.Types.ToList();
            if (types.Any(t => t.Type == "new-content"))
            {
                return RedirectToAction("Content", new { message = EditorMessage.Exists });
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
                ShowMeta = true,
                Gallery = false,
                Templates = false,
                TemplateFolder = "Templates",
                CustomFields = ContentTypes.BaseFields(new List<CustomField>())
            });
            model.Types = types.ToArray();
            model.CheckBaseFields();

            _settings.Set("Hood.Settings.Content", model);

            // refresh all content metas and things
            _content.RefreshAllMetas();

            model.SaveMessage = "Settings saved!";
            model.MessageType = Enums.AlertType.Success;

            return RedirectToAction("Content", new { message = EditorMessage.Created });
        }
        [Route("admin/settings/content/delete-type/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DeleteContentType(string type)
        {   
            ContentSettings model = _settings.GetContentSettings(true);

            if (model == null)
                model = new ContentSettings();

            var types = model.Types.ToList();
            if (!types.Any(t => t.Type == type))
            {
                return RedirectToAction("Content", new { message = EditorMessage.Deleted });
            }
            types.RemoveAll(t => t.Type == type);
            model.Types = types.ToArray();
            model.CheckBaseFields();

            _settings.Set("Hood.Settings.Content", model);

            // refresh all content metas and things
            _content.RefreshAllMetas();

            model.SaveMessage = "Settings saved!";
            model.MessageType = Enums.AlertType.Success;

            return RedirectToAction("Content", new { message = EditorMessage.Created });
        }

        [Route("admin/settings/property/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Property()
        {
            PropertySettings model = _settings.GetPropertySettings(true);
            if (model == null)
                model = new PropertySettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/property/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Property(PropertySettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Property", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/property/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetProperty()
        {
            var model = new PropertySettings();
            _settings.Set("Hood.Settings.Property", model);
            return RedirectToAction("Property");
        }


        [Route("admin/settings/billing/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Billing()
        {
            BillingSettings model = _settings.GetBillingSettings(true);
            if (model == null)
                model = new BillingSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/billing/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Billing(BillingSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Billing", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/billing/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetBilling()
        {
            var model = new BillingSettings();
            _settings.Set("Hood.Settings.Billing", model);
            return RedirectToAction("Billing");
        }

        [Route("admin/settings/account/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Account()
        {
            AccountSettings model = _settings.GetAccountSettings(true);
            if (model == null)
                model = new AccountSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/account/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Account(AccountSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Account", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/account/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetAccount()
        {
            var model = new AccountSettings();
            _settings.Set("Hood.Settings.Account", model);
            return RedirectToAction("Account");
        }


        [Route("admin/settings/seo/")]
        [Authorize(Roles = "Admin,Manager,Editor")]
        public IActionResult Seo()
        {
            SeoSettings model = _settings.GetSeo(true);
            if (model == null)
                model = new SeoSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/seo/")]
        [Authorize(Roles = "Admin,Manager,Editor")]
        public IActionResult Seo(SeoSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Seo", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/seo/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetSeo()
        {
            var model = new SeoSettings();
            _settings.Set("Hood.Settings.Seo", model);
            return RedirectToAction("Seo");
        }


        [Route("admin/settings/media/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Media()
        {
            MediaSettings model = _settings.GetMediaSettings(true);
            if (model == null)
                model = new MediaSettings();

            model.UpdateReport = _mediaRefresh.Report();

            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/media/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Media(MediaSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Media", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }

            model.UpdateReport = _mediaRefresh.Report();

            return View(model);
        }

        [Route("admin/settings/media/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetMedia()
        {
            var model = new MediaSettings();
            _settings.Set("Hood.Settings.Media", model);
            return RedirectToAction("Media");
        }
        [Route("admin/settings/media/refresh/")]

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult RefreshMedia()
        {
            _mediaRefresh.Kill();
            _mediaRefresh.RunUpdate(HttpContext);
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Mail(EditorMessage? status)
        {
            MailSettings model = _settings.GetMailSettings(true);
            if (model == null)
                model = new MailSettings();
            model.AddEditorMessage(status);
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/mail/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Mail(MailSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Mail", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("admin/settings/mail/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetMail()
        {
            var model = new MailSettings();
            _settings.Set("Hood.Settings.Mail", model);
            return RedirectToAction("Mail");
        }

        [Route("admin/settings/forum/")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Forum(EditorMessage? status)
        {
            ForumSettings model = _settings.GetForumSettings(true);
            if (model == null)
                model = new ForumSettings();
            model.AddEditorMessage(status);
            model.Subscriptions = await _auth.GetSubscriptionPlansAsync();
            model.Roles = _auth.GetAllRoles();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/forum/")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Forum(ForumSettings model)
        {
            try
            {
                _settings.Set("Hood.Settings.Forum", model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            model.Subscriptions = await _auth.GetSubscriptionPlansAsync();
            model.Roles = _auth.GetAllRoles();
            return View(model);
        }

        [Route("admin/settings/forum/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetForum()
        {
            var model = new ForumSettings();
            _settings.Set("Hood.Settings.Forum", model);
            return RedirectToAction("Forum");
        }


        [Route("admin/settings/advanced/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Advanced()
        {
            return View();
        }

        #region "Caching" 
        [Route("admin/settings/removecacheitem/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult RemoveCacheItem(string key)
        {
            _cache.Remove(key);
            return RedirectToAction("Advanced");
        }
        [Route("admin/settings/resetcache/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetCache()
        {
            _cache.ResetCache();
            return RedirectToAction("Advanced");
        }
        #endregion

    }
}
