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
    }

}
