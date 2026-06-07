using System;
using System.ComponentModel.DataAnnotations;
using Hood.BaseTypes;
using Hood.Extensions;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [Serializable]
    public class IntegrationSettings : SaveableModel
    {
        // Google Maps Api
        [Display(Name = "Enable Google Geocoding (Location finding for addresses)")]
        public bool EnableGoogleGeocoding { get; set; }

        [Display(Name = "Enable Google Maps")]
        public bool EnableGoogleMaps { get; set; }

        [Display(Name = "Google API Key")]
        public string GoogleMapsApiKey { get; set; }

        public bool IsGoogleMapsEnabled
        {
            get { return GoogleMapsApiKey.IsSet() && EnableGoogleMaps; }
        }
        public bool IsGoogleGeocodingEnabled
        {
            get { return GoogleMapsApiKey.IsSet() && EnableGoogleGeocoding; }
        }
        public bool IsGoogleRecaptchaEnabled
        {
            get
            {
                return GoogleRecaptchaSiteKey.IsSet()
                    && GoogleRecaptchaProjectId.IsSet()
                    && GoogleRecaptchaApiKey.IsSet()
                    && EnableGoogleRecaptcha;
            }
        }

        // Google Analytics
        [Display(Name = "Google Analytics Code")]
        public string GoogleAnalytics { get; set; }

        // Google Recaptcha (Fraud Defence / reCAPTCHA Enterprise assessment API)
        [Display(Name = "Enable Google Recaptcha")]
        public bool EnableGoogleRecaptcha { get; set; }

        [Display(Name = "Google Recaptcha Site Key")]
        public string GoogleRecaptchaSiteKey { get; set; }

        [Display(Name = "Google Cloud Project Id")]
        public string GoogleRecaptchaProjectId { get; set; }

        [Display(Name = "Google Cloud API Key")]
        public string GoogleRecaptchaApiKey { get; set; }

        [Display(Name = "Google Recaptcha Security Threshold")]
        public decimal GoogleRecaptchaThreshold { get; set; }

        // Unsplash Api
        [Display(Name = "Unsplash Access Key")]
        public string UnsplashAccessKey { get; set; }

        [Display(Name = "Unsplash Secret Key")]
        public string UnsplashSecretKey { get; set; }
    }
}
