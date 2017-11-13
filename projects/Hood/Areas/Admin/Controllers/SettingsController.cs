using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Hood.Services;
using Hood.Models;
using System;
using Hood.Caching;
using Geocoding.Google;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class SettingsController : Controller
    {
        public readonly IConfiguration _config;
        public readonly IHostingEnvironment _env;
        private readonly IContentRepository _content;
        private readonly ISettingsRepository _settings;
        private readonly IAccountRepository _auth;
        private readonly IAddressService _address;
        private readonly IHoodCache _cache;
        private readonly IMediaManager<MediaObject> _media;
        private readonly HoodDbContext _db;
        private readonly IEmailSender _email;
        private readonly IMediaRefreshService _mediaRefresh;

        public SettingsController(IAccountRepository auth,
                              IConfiguration conf,
                              IHostingEnvironment env,
                              ISettingsRepository site,
                              IContentRepository content,
                              IAddressService address,
                              IHoodCache cache,
                              IMediaManager<MediaObject> media,
                              HoodDbContext db,
                              IEmailSender email,
                              IMediaRefreshService mediaRefresh)
        {
            _auth = auth;
            _config = conf;
            _env = env;
            _content = content;
            _address = address;
            _settings = site;
            _cache = cache;
            _media = media;
            _db = db;
            _email = email;
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
                    model.Address.Latitude = location.Latitude;
                    model.Address.Longitude = location.Longitude;
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
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetContact()
        {
            var model = new ContactSettings();
            _settings.Set("Hood.Settings.Contact", model);
            return RedirectToAction("Contact");
        }


        [Route("admin/settings/content/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Content()
        {
            ContentSettings model = _settings.GetContentSettings(true);
            if (model == null)
                model = new ContentSettings();
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetContent()
        {
            var model = new ContentSettings();
            _settings.Set("Hood.Settings.Content", model);
            // refresh all content metas and things
            _content.RefreshAllMetas();
            return RedirectToAction("Content");
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetProperty()
        {
            var model = new PropertySettings();
            _settings.Set("Hood.Settings.Property", model);
            return RedirectToAction("Property");
        }


        [Route("admin/settings/billing/")]
        [Authorize(Roles = "Admin")]
        public IActionResult Billing()
        {
            BillingSettings model = _settings.GetBillingSettings(true);
            if (model == null)
                model = new BillingSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/billing/")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetBilling()
        {
            var model = new BillingSettings();
            _settings.Set("Hood.Settings.Billing", model);
            return RedirectToAction("Billing");
        }

        [Route("admin/settings/account/")]
        [Authorize(Roles = "Admin")]
        public IActionResult Account()
        {
            AccountSettings model = _settings.GetAccountSettings(true);
            if (model == null)
                model = new AccountSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/account/")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetAccount()
        {
            var model = new AccountSettings();
            _settings.Set("Hood.Settings.Account", model);
            return RedirectToAction("Account");
        }


        [Route("admin/settings/seo/")]
        [Authorize(Roles = "Admin,Manager,SEO")]
        public IActionResult Seo()
        {
            SeoSettings model = _settings.GetSeo(true);
            if (model == null)
                model = new SeoSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/seo/")]
        [Authorize(Roles = "Admin,Manager,SEO")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetSeo()
        {
            var model = new SeoSettings();
            _settings.Set("Hood.Settings.Seo", model);
            return RedirectToAction("Seo");
        }


        [Route("admin/settings/media/")]
        [Authorize(Roles = "Admin,Editor,Manager")]
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
        [Authorize(Roles = "Admin,Editor,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Editor,Manager")]
        public IActionResult Mail()
        {
            MailSettings model = _settings.GetMailSettings(true);
            if (model == null)
                model = new MailSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/mail/")]
        [Authorize(Roles = "Admin,Editor,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetMail()
        {
            var model = new MailSettings();
            _settings.Set("Hood.Settings.Mail", model);
            return RedirectToAction("Mail");
        }


        [Route("admin/settings/advanced/")]
        [Authorize(Roles = "Admin")]
        public IActionResult Advanced()
        {
            return View();
        }

        #region "Caching" 
        public IActionResult RemoveCacheItem(string key)
        {
            _cache.Remove(key);
            return RedirectToAction("Advanced");
        }
        public IActionResult ResetCache()
        {
            _cache.ResetCache();
            return RedirectToAction("Advanced");
        }
        #endregion

    }
}
