using Hood.BaseTypes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [Serializable]
    public class IntegrationSettings : SaveableModel
    {
        // Loading Animations
        [Display(Name = "Enable Loading Animation")]
        public bool LoadingAnimation { get; set; }
        [Display(Name = "Loading Animation")]
        public string LoadingAnimationType { get; set; }

        [Display(Name = "Use Hood CDN")]
        public bool UseCDN { get; set; }

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

        // Google Maps Api
        [Display(Name = "Enable Google Geocoding (Location finding for addresses)")]
        public bool EnableGoogleGeocoding { get; set; }
        [Display(Name = "Enable Google Maps")]
        public bool EnableGoogleMaps { get; set; }
        [Display(Name = "Google API Key")]
        public string GoogleMapsApiKey { get; set; }

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

        // Mailchimp
        [Display(Name = "Enable Mailchimp")]
        public bool EnableMailchimp { get; set; }
        [Display(Name = "Mailchimp Api Key")]
        public string MailchimpApiKey { get; set; }
        public string MailchimpDataCenter
        {
            get
            {
                return string.IsNullOrWhiteSpace(MailchimpApiKey)
                    ? string.Empty
                    : MailchimpApiKey.Substring(
                        MailchimpApiKey.LastIndexOf("-", StringComparison.Ordinal) + 1,
                        MailchimpApiKey.Length - MailchimpApiKey.LastIndexOf("-", StringComparison.Ordinal) - 1);
            }
        }
        public string MailchimpAuthHeader => $"apikey {MailchimpApiKey}";
        [Display(Name = "Mailchimp List Id")]
        public string MailchimpListId { get; set; }

        public IntegrationSettings()
        {
            UseCDN = false;
        }
    }

}
