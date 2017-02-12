using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Hood.Services;
using Hood.Models;
using System;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class SettingsController : Controller
    {
        public readonly IConfiguration _config;
        public readonly IHostingEnvironment _env;
        private readonly IContentRepository _content;
        private readonly ISiteConfiguration _site;
        private readonly IAuthenticationRepository _auth;
        private readonly IAddressService _address;

        public SettingsController(IAuthenticationRepository auth,
                              IConfiguration conf,
                              IHostingEnvironment env,
                              ISiteConfiguration site,
                              IContentRepository content,
                              IAddressService address)
        {
            _auth = auth;
            _config = conf;
            _env = env;
            _content = content;
            _address = address;
            _site = site;
        }

        [Route("admin/settings/basics/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Basics()
        {
            BasicSettings model = _site.GetBasicSettings(true);
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
                var location = _address.GeocodeAddress(model.Address);
                if (location != null)
                {
                    model.Address.Latitude = location.Latitude;
                    model.Address.Longitude = location.Longitude;
                    _site.Set("Hood.Settings.Basic", model);
                    model.SaveMessage = "Settings saved!";
                    model.MessageType = Enums.AlertType.Success;
                }
                else
                {
                    _site.Set("Hood.Settings.Basic", model);
                    model.SaveMessage = "Settings were saved, but because there was an error with the Google API, your address could not be located on the map. Check your Google API key in your Integration Settings, and ensure your API key has the Geocoding API enabled.";
                    model.MessageType = Enums.AlertType.Warning;
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
            _site.Set("Hood.Settings.Basic", model);
            return RedirectToAction("Basics");
        }


        [Route("admin/integrations/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Integrations()
        {
            IntegrationSettings model = _site.GetIntegrationSettings(true);
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
                _site.Set("Hood.Settings.Integrations", model);
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
            _site.Set("Hood.Settings.Integrations", model);
            return RedirectToAction("Integrations");
        }


        [Route("admin/settings/contact/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Contact()
        {
            ContactSettings model = _site.GetContactSettings(true);
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
                _site.Set("Hood.Settings.Contact", model);
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
            _site.Set("Hood.Settings.Contact", model);
            return RedirectToAction("Contact");
        }


        [Route("admin/settings/content/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Content()
        {
            ContentSettings model = _site.GetContentSettings(true);
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
                _site.Set("Hood.Settings.Content", model);
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
            var model = new ContactSettings();
            _site.Set("Hood.Settings.Content", model);
            return RedirectToAction("Content");
        }


        [Route("admin/settings/property/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Property()
        {
            PropertySettings model = _site.GetPropertySettings(true);
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
                _site.Set("Hood.Settings.Property", model);
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
            _site.Set("Hood.Settings.Property", model);
            return RedirectToAction("Property");
        }


        [Route("admin/settings/billing/")]
        [Authorize(Roles = "Admin")]
        public IActionResult Billing()
        {
            BillingSettings model = _site.GetBillingSettings(true);
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
                _site.Set("Hood.Settings.Billing", model);
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
            _site.Set("Hood.Settings.Billing", model);
            return RedirectToAction("Billing");
        }


        [Route("admin/settings/seo/")]
        [Authorize(Roles = "Admin,Manager,SEO")]
        public IActionResult Seo()
        {
            SeoSettings model = _site.GetSeo(true);
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
                _site.Set("Hood.Settings.Seo", model);
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
            _site.Set("Hood.Settings.Seo", model);
            return RedirectToAction("Seo");
        }


        [Route("admin/settings/media/")]
        [Authorize(Roles = "Admin,Editor,Manager")]
        public IActionResult Media()
        {
            MediaSettings model = _site.GetMediaSettings(true);
            if (model == null)
                model = new MediaSettings();
            return View(model);
        }
        [HttpPost]
        [Route("admin/settings/media/")]
        [Authorize(Roles = "Admin,Editor,Manager")]
        public IActionResult Media(MediaSettings model)
        {
            try
            {
                _site.Set("Hood.Settings.Media", model);
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
        [Route("admin/settings/media/reset/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ResetMedia()
        {
            var model = new MediaSettings();
            _site.Set("Hood.Settings.Media", model);
            return RedirectToAction("Media");
        }


        [Route("admin/settings/mail/")]
        [Authorize(Roles = "Admin,Editor,Manager")]
        public IActionResult Mail()
        {
            MailSettings model = _site.GetMailSettings(true);
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
                _site.Set("Hood.Settings.Mail", model);
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
            _site.Set("Hood.Settings.Mail", model);
            return RedirectToAction("Mail");
        }


        [Route("admin/settings/advanced/")]
        [Authorize(Roles = "Admin")]
        public IActionResult Advanced()
        {
            return View();
        }

    }
}
