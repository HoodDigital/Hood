using Geocoding.Google;
using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            _cache.Remove(typeof(BasicSettings).ToString());
            BasicSettings model = Engine.Settings.Basic;
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
                Engine.Settings.Set(model);
                var location = _address.GeocodeAddress(model.Address);
                if (location != null)
                {
                    model.Address.Latitude = location.Coordinates.Latitude;
                    model.Address.Longitude = location.Coordinates.Longitude;
                    Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
            return RedirectToAction("Basics");
        }


        [Route("admin/integrations/")]
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Integrations(IntegrationSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
            return RedirectToAction("Integrations");
        }


        [Route("admin/settings/contact/")]
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Contact(ContactSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
            return RedirectToAction("Contact");
        }


        [Route("admin/settings/content/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Content(EditorMessage? message)
        {
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings model = Engine.Settings.Content;
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

                Engine.Settings.Set(model);

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
            Engine.Settings.Set(model);
            // refresh all content metas and things
            _content.RefreshAllMetas();
            return RedirectToAction("Content");
        }
        [Route("admin/settings/content/add-type/")]

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AddContentType()
        {
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings model = Engine.Settings.Content;
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

            Engine.Settings.Set(model);

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
            _cache.Remove(typeof(ContentSettings).ToString());
            ContentSettings model = Engine.Settings.Content;

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

            Engine.Settings.Set(model);

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
            _cache.Remove(typeof(PropertySettings).ToString());
            PropertySettings model = Engine.Settings.Property;
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
                Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
            return RedirectToAction("Property");
        }


        [Route("admin/settings/billing/")]
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Billing(BillingSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
            return RedirectToAction("Billing");
        }

        [Route("admin/settings/account/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Account()
        {
            _cache.Remove(typeof(AccountSettings).ToString());
            AccountSettings model = Engine.Settings.Account;
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
                Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
            return RedirectToAction("Account");
        }


        [Route("admin/settings/seo/")]
        [Authorize(Roles = "Admin,Manager,Editor")]
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
        [Authorize(Roles = "Admin,Manager,Editor")]
        public IActionResult Seo(SeoSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
            return RedirectToAction("Seo");
        }


        [Route("admin/settings/media/")]
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Media(MediaSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
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
            _cache.Remove(typeof(MailSettings).ToString());
            MailSettings model = Engine.Settings.Mail;
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
                Engine.Settings.Set(model);
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
            Engine.Settings.Set(model);
            return RedirectToAction("Mail");
        }

        [Route("admin/settings/forum/")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Forum(EditorMessage? status)
        {
            _cache.Remove(typeof(ForumSettings).ToString());
            ForumSettings model = Engine.Settings.Forum;
            if (model == null)
                model = new ForumSettings();
            model.AddEditorMessage(status);
            model.Subscriptions = await _account.GetSubscriptionPlansAsync();
            model.Roles = _account.GetAllRoles();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/forum/")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Forum(ForumSettings model)
        {
            try
            {
                Engine.Settings.Set(model);
                model.SaveMessage = "Settings saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            model.Subscriptions = await _account.GetSubscriptionPlansAsync();
            model.Roles = _account.GetAllRoles();
            return View(model);
        }

        [Route("admin/settings/forum/reset/")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetForum()
        {
            var model = new ForumSettings();
            Engine.Settings.Set(model);
            return RedirectToAction("Forum");
        }


        [Route("admin/settings/advanced/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Advanced()
        {
            return View();
        }

        #region Caching 
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
