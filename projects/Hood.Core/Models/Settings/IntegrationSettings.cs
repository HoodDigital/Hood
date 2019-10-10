using Hood.BaseTypes;
using Hood.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [Serializable]
    public class IntegrationSettings : SaveableModel
    {
        // Twitter Feed
        [Display(Name = "Twitter Feed")]
        public bool EnableTwitter { get; set; }
        [Display(Name = "Twitter Handle")]
        public string TwitterId { get; set; }

        // Disqus
        [Display(Name = "Enable Disqus")]
        public bool EnableDisqus { get; set; }
        [Display(Name = "Disqus ID")]
        public string DisqusId { get; set; }

        // Mailchimp
        [Display(Name = "Enable Mailchimp")]
        public bool EnableMailchimp { get; set; }
        [Display(Name = "Mailchimp Api Key")]
        public string MailchimpApiKey { get; set; }
        [Display(Name = "Mailchimp User List Id (Sync your site users to this list)")]
        public string MailchimpUserListId { get; set; }

        // Google Maps Api
        [Display(Name = "Enable Google Geocoding (Location finding for addresses)")]
        public bool EnableGoogleGeocoding { get; set; }
        [Display(Name = "Enable Google Maps")]
        public bool EnableGoogleMaps { get; set; }
        [Display(Name = "Google API Key")]
        public string GoogleMapsApiKey { get; set; }

        public bool IsGoogleMapsEnabled
        {
            get
            {
                return GoogleMapsApiKey.IsSet() && EnableGoogleMaps;
            }
        }
        public bool IsGoogleGeocodingEnabled
        {
            get
            {
                return GoogleMapsApiKey.IsSet() && EnableGoogleGeocoding;
            }
        }
        public bool IsGoogleRecaptchaEnabled
        {
            get
            {
                return GoogleRecaptchaSiteKey.IsSet() && GoogleRecaptchaSecretKey.IsSet() && EnableGoogleRecaptcha;
            }
        }

        // Google Analytics
        [Display(Name = "Google Analytics Code")]
        public string GoogleAnalytics { get; set; }

        // Google Recaptcha
        [Display(Name = "Enable Google Recaptcha")]
        public bool EnableGoogleRecaptcha { get; set; }
        [Display(Name = "Google Recaptcha Site Key")]
        public string GoogleRecaptchaSiteKey { get; set; }
        [Display(Name = "Google Recaptcha Secret Key")]
        public string GoogleRecaptchaSecretKey { get; set; }

        // Disqus
        [Display(Name = "Enable Chat")]
        public bool EnableChat { get; set; }
        [Display(Name = "Script for Chat Plugin")]
        public string ChatCode { get; set; }
    }

}
