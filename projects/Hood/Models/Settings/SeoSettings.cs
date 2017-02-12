using Hood.BaseTypes;
using Hood.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [Serializable]
    public class SeoSettings : SaveableModel
    {
        // Site Socials
        [Display(Name = "Twitter URL")]
        public string Twitter { get; set; }
        [Display(Name = "Twitter Handle (@yourname)")]
        public string TwitterHandle { get; set; }
        [Display(Name = "Facebook URL")]
        public string Facebook { get; set; }
        [Display(Name = "Google+ URL")]
        public string GooglePlus { get; set; }
        [Display(Name = "LinkedIn URL")]
        public string LinkedIn { get; set; }
        [Display(Name = "TripAdvisor URL")]
        public string TripAdvisor { get; set; }
        [Display(Name = "Github URL")]
        public string GitHub { get; set; }
        [Display(Name = "Instagram URL")]
        public string Instagram { get; set; }
        [Display(Name = "Pinterest URL")]
        public string Pinterest { get; set; }
        
        public bool HasSocials()
        {
            return Twitter.IsSet() || Facebook.IsSet() || GooglePlus.IsSet() || LinkedIn.IsSet() || GitHub.IsSet() || Instagram.IsSet() || Pinterest.IsSet();
        }

        // basic
        [Display(Name = "Google Analytics Code")]
        public string GoogleAnalytics { get; set; }
        [Display(Name = "Facebook Application ID")]
        public string FacebookAppId { get; set; }
        [Display(Name = "Site Icon (favicon) Url")]
        public string SiteIconUrl { get; set; }
        [Display(Name = "Google+ Publisher Url")]
        public string Publisher { get; set; }
        [Display(Name = "Canonical Url")]
        public string CanonicalUrl { get; set; }
        [Display(Name = "Default Page Title")]
        public string Title { get; set; }
        [Display(Name = "Keywords")]
        public string Keywords { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "OG:Url")]
        public string OgUrl { get; set; }
        [Display(Name = "OG:Type")]
        public string OgType { get; set; }
        [Display(Name = "OG:Author")]
        public string OgAuthor { get; set; }
        [Display(Name = "OG:Title")]
        public string OgTitle { get; set; }
        [Display(Name = "OG:Image Url")]
        public string OgImageUrl { get; set; }
        [Display(Name = "OG:Secure Image Url")]
        public string OgSecureImageUrl { get; set; }
        [Display(Name = "OG:Locale")]
        public string OgLocale { get; set; }
        [Display(Name = "OG:Description")]
        public string OgDescription { get; set; }
        
        [Display(Name = "Twitter Card: Site")]
        public string TwitterCardSite { get; set; }
        [Display(Name = "Twitter Card: Creator")]
        public string TwitterCardCreator { get; set; }
        [Display(Name = "Twitter Card: Title")]
        public string TwitterCardTitle { get; set; }
        [Display(Name = "Twitter Card: Description")]
        public string TwitterCardDescription { get; set; }
        [Display(Name = "Twitter Card: Image Url")]
        public string TwitterCardImageUrl { get; set; }

        public PageSeo Home { get; set; }
        public PageSeo Contact { get; set; }
        public PageSeo Login { get; set; }
        public PageSeo SignUp { get; set; }

    }

    public class PageSeo
    {
        [Display(Name = "Canonical Url")]
        public string CanonicalUrl { get; set; }
        [Display(Name = "Page Title")]
        public string Title { get; set; }
        [Display(Name = "Keywords")]
        public string Keywords { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
