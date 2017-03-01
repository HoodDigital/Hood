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

        // Show Theme Demo Page
        [Display(Name = "Enable the Boostrap Demo Page")]
        public bool ShowDemoPage { get; set; }

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
        [Display(Name = "Mailchimp List Id")]
        public string MailchimpListId { get; set; }

        public IntegrationSettings()
        {
        }
    }

}
